using Binder.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.IO;
using Binder.Parameter;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using Prism.Mvvm;
using System.Threading;
using Prism.Commands;
using System.Windows.Data;
using System.Windows.Input;
using Binder.MyWindow.Models;

namespace Binder.MyWindow.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private MainWindowModel _model = new MainWindowModel();
        private CancellationTokenSource _cancelTokenSource = null;
        private ExportSettings _exportParameter = new ExportSettings();
        private DispatcherTimer _timer = new DispatcherTimer();
        public enum eFilterEnum
        {
            All = 0, Excel, ExcelMacro, Word
        }

        #region プロパティ (prism使用)
        private bool _isPdfCombine = false;
        public bool IsPdfCombine
        {
            get { return this._isPdfCombine; }
            set { SetProperty(ref _isPdfCombine, value); }
        }

        public Dictionary<eFilterEnum, string> FilterEnumNameDictionary
        { get; } = new Dictionary<eFilterEnum, string>();


        ObservableCollection<FileInformation> _fileInformationList = new System.Collections.ObjectModel.ObservableCollection<FileInformation>();
        public ObservableCollection<FileInformation> FileInformationList
        {
            get { return this._fileInformationList; }
            set { SetProperty(ref _fileInformationList, value); }
        }

        ObservableCollection<string> _logList = new ObservableCollection<string>();
        public ObservableCollection<string> LogList
        {
            get { return this._logList; }
            set { SetProperty(ref _logList, value); }
        }

        private int _progressMax;
        public int ProgressMax
        {
            get { return this._progressMax; }
            set { SetProperty(ref _progressMax, value); }
        }

        private int _progressValue;
        public int ProgressValue
        {
            get { return this._progressValue; }
            set { SetProperty(ref _progressValue, value); }
        }

        private bool _isOutputProcess;
        public bool IsOutputProcess
        {
            get { return this._isOutputProcess; }
            set { SetProperty(ref _isOutputProcess, value); }
        }

        private string _outputFileName;
        public string OutputFileName
        {
            get { return this._outputFileName; }
            set { SetProperty(ref _outputFileName, value); }
        }

        private string _outputDirectory;
        public string OutputDirectory
        {
            get { return this._outputDirectory; }
            set { SetProperty(ref _outputDirectory, value); }
        }
        #endregion

        #region デリゲートコマンド
        private DelegateCommand _exportCommand;
        public DelegateCommand ExportCommand
        {
            get { return _exportCommand = _exportCommand ?? new DelegateCommand(Export); }
        }
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindowViewModel()
        {
            this._model = new MainWindowModel();
            // プロパティ初期化
            this.ProgressMax = 100;
            this.ProgressValue = 0;            
            this.IsOutputProcess = false;
            DateTime time = System.DateTime.Today;
            this.OutputFileName = time.ToString("yyyyMMdd");
            this.OutputDirectory = System.AppDomain.CurrentDomain.BaseDirectory + "export" + "\\";
            this.IsPdfCombine = true;

            // コンボボックス表示アイテム定義
            //this.FilterEnumNameDictionary.Add(eFilterEnum.All, "全て(*.*)");
            //this.FilterEnumNameDictionary.Add(eFilterEnum.Excel, "エクセルブック(*.xlsx)");
            this.FilterEnumNameDictionary.Add(eFilterEnum.ExcelMacro, "マクロ有効ブック(*.xlsm)");
            //this.FilterEnumNameDictionary.Add(eFilterEnum.Word, "ワード文書(*.docx)");

            // 複数スレッドからコレクション操作できるようにする
            BindingOperations.EnableCollectionSynchronization(this.FileInformationList, new object());
            BindingOperations.EnableCollectionSynchronization(this.LogList, new object());
        }

        public string ConvertSearchPatternString(object filter)
        {
            eFilterEnum selectFilter = (eFilterEnum)filter;
            switch ((int)filter)
            {
                case (int)eFilterEnum.All: return "*";
                case (int)eFilterEnum.Excel: return "*.xlsx";
                case (int)eFilterEnum.ExcelMacro: return "*.xlsm";
                case (int)eFilterEnum.Word: return "*.docx";
                default: return "*";
            }
        }

        // リスト初期化
        public void InitializeList()
        {
            this.FileInformationList.Clear();
        }

        // ファイルリストの取得
        public ObservableCollection<FileInformation> GetFileInformationList()
        {
            return this.FileInformationList;
        }

        // ファイルリストの差しかえ
        public void ChangeFilesInformationList(ObservableCollection<FileInformation> list)
        {
            this.InitializeList();
            foreach (var file in list)
                this.AddCollection(file);
        }

        public void RemoveCollection(FileInformation item)
        {
            this.FileInformationList.Remove(item);
            this.AddLog(item.FileName + "読込削除");
        }
        
        public void InsertCollection(int index, FileInformation item)
        {
            this.FileInformationList.Remove(item);
            this.FileInformationList.Insert(index, item);
        }

        public void AddCollection(FileInformation item)
        {
            this.FileInformationList.Remove(item);
            this.FileInformationList.Add(item);
        }

        // ファイルパラメータに変換
        public void ConvertToFileInformationList(List<string> pathList)
        {
            foreach (var item in pathList.Select((value, index) => new { value, index }))
            {
                // 生成
                FileInformation fileInformation = new FileInformation();
                fileInformation.FileName = Path.GetFileName(item.value);
                fileInformation.ParentFolder = Path.GetFileName(Path.GetDirectoryName(item.value));
                fileInformation.FilePath = item.value;
                fileInformation.Format = Path.GetExtension(item.value);

                // リストに追加
                this.FileInformationList.Add(fileInformation);
            }
            this.AddLog(pathList.Count.ToString() + "ファイル読み込みました。");
        }

        private ExportSettings InitializeExportParameter()
        {
            ExportSettings parameter = new ExportSettings();
            if (IsPdfCombine == true) parameter.ExportFormat = ExportSettings.eExportFormat.PdfBinder;
            else parameter.ExportFormat = ExportSettings.eExportFormat.PdfFile;

            parameter.OutputFolderPath = this.OutputDirectory;

            parameter.OutputFileName = this.OutputFileName + ".pdf";

            return parameter;
        }

        /// <summary>
        /// 出力処理を実行する。 
        /// </summary>
        private async void Export() // async void は好ましくない。UIからのイベントによって実行されるメソッドなので仕方なし。
        {
            IsOutputProcess = true;
            this.AddLog("出力処理を開始します。（ファイル数：" + this.FileInformationList.Count.ToString() + "）");
            ExportSettings exportParameter = InitializeExportParameter(); // 出力設定パラメータをインスタンス

            _timer = new DispatcherTimer(DispatcherPriority.Normal);
            _timer.Interval = new TimeSpan(0, 0, 0, 10, 0);
            _timer.Tick += new EventHandler(ExportTimer_Tick);
            _timer.Start();

            List<string> filePaths = new List<string>(); // 入力ファイルパスリスト取得
            foreach (var information in FileInformationList)
            {
                filePaths.Add(information.FilePath);
            }

            // 出力
            using (_cancelTokenSource = new CancellationTokenSource())
            {
                var result = await _model.FunctionExportAsync(exportParameter, filePaths, _cancelTokenSource.Token);
            }
            
            _timer.Stop();

            this.ProgressValue = this._model.GetExportProgress();

            // 完了
            MessageBox.Show("出力完了");
            this.AddLog("出力処理が終了しました。");
            IsOutputProcess = false;
        }

        private void ExportTimer_Tick(object sender, EventArgs e)
        {
            // Get Progress
            this.ProgressValue = this._model.GetExportProgress();
        }
                
        // 出力メッセージを生成
        private string CreateCompleteMessage(int errorDocuments, int errorAddFiles, CancellationToken token)
        {
            string message = "";

            if (token.IsCancellationRequested)
                message = "処理を中断しました。";
            else if (errorDocuments > 0 || errorAddFiles > 0)
            {
                message = "出力処理が終了しました。" + Environment.NewLine + Environment.NewLine;
                message += "DocuWorks文書印刷エラー数：" + errorDocuments.ToString() + "ファイル" + Environment.NewLine;
                message += "バインダー追加エラー数：" + errorAddFiles.ToString() + "ファイル";
            }
            else
                message = "出力処理が正常終了しました。";
            return message;
        }

        // キャンセルを受付
        public void CancelOn()
        {
            if (this._cancelTokenSource != null)
            {
                this._cancelTokenSource.Cancel();
                this.AddLog("キャンセルをクリックしました。");
            }
        }

        // ログListBoxに任意の文字を表示
        private void AddLog(string str)
        {
            var time = System.DateTime.Now;
            this.LogList.Add(time.ToString() + "\t: " + str);
        }

        // 除外リストを編集
        public void EditNotTarget()
        {
            string path = this._model.GetNotTargetFileName();
            System.Diagnostics.Process.Start("notepad.exe", path);
        }
            
    }

    
}
