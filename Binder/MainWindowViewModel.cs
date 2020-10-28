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

namespace Binder
{
    public class MainWindowViewModel : BindableBase
    {
        #region ---------- Field ------------------------------------------------------------------------------------------
        public enum eFilterEnum
        {
            All = 0, Excel, ExcelMacro, Word
        }
        private MainWindowModel _model = new MainWindowModel();
        private CancellationTokenSource _cancelTokenSource = null;
        #endregion ======= Field ==========================================================================================


        #region ---------- Construction ------------------------------------------------------------------------------------------
        // コンストラクタ
        public MainWindowViewModel()
        {
            // フィールド初期化
            this._model = new MainWindowModel();

            // プロパティ初期化
            this.ProgressMax = 1;
            this.ProgressValue = 0;
            this.IsOutputProcess = false;
            DateTime time = System.DateTime.Today;
            this.OutputFileName = time.ToString("yyyyMMdd");
            this.OutputDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            this.IsXdwSave = false;

            // フィルタコンボボックス表示アイテム定義
            this.FilterEnumNameDictionary.Add(eFilterEnum.All, "全て(*.*)");
            this.FilterEnumNameDictionary.Add(eFilterEnum.Excel, "エクセルブック(*.xlsx)");
            this.FilterEnumNameDictionary.Add(eFilterEnum.ExcelMacro, "マクロ有効ブック(*.xlsm)");
            this.FilterEnumNameDictionary.Add(eFilterEnum.Word, "ワード文書(*.docx)");

            // 複数スレッドからコレクション操作できるようにする
            BindingOperations.EnableCollectionSynchronization(this.FileInformationList, new object());
            BindingOperations.EnableCollectionSynchronization(this.LogList, new object());
        }
        #endregion ======= Construction ==========================================================================================


        #region ---------- Property ------------------------------------------------------------------------------------------
        public Dictionary<eFilterEnum, string> FilterEnumNameDictionary { get; } = new Dictionary<eFilterEnum, string>();

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

        private bool _isXdwSave;
        public bool IsXdwSave
        {
            get { return this._isXdwSave; }
            set { SetProperty(ref _isXdwSave, value); }
        }
        #endregion ======= Property ==========================================================================================

        // todo: impl
        #region ---------- Command ------------------------------------------------------------------------------------------
        //public DelegateCommand ExecuteDelegateCommand { get; private set; }
        //public DelegateCommand<string> ExecuteGenericDelegateCommand { get; private set; }
        //public DelegateCommand DelegateCommandObservesProperty { get; private set; }
        //public DelegateCommand DelegateCommandObservesCanExecute { get; private set; }

        //private bool _isEnabled;
        //public bool IsEnabled
        //{
        //    get { return _isEnabled; }
        //    set
        //    {
        //        SetProperty(ref _isEnabled, value);
        //        ExecuteDelegateCommand.RaiseCanExecuteChanged();
        //    }
        //}

        //private string _updateText;
        //public string UpdateText
        //{
        //    get { return _updateText; }
        //    set { SetProperty(ref _updateText, value); }
        //}

        //private void Execute()
        //{
        //    UpdateText = $"Updated: {DateTime.Now}";
        //}

        //private void ExecuteGeneric(string parameter)
        //{
        //    UpdateText = parameter;
        //}

        //private bool CanExecute()
        //{
        //    return IsEnabled;
        //}
        #endregion ======= Command ==========================================================================================


        #region ---------- Method: Control Operation ------------------------------------------------------------------------------------------
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

        // リストからアイテムを削除する
        public void RemoveCollection(FileInformation item)
        {
            this.FileInformationList.Remove(item);
            this.AddLog(item.FileName + "読込削除");
        }

        // リストにアイテムを挿入する
        public void InsertCollection(int index, FileInformation item)
        {
            this.FileInformationList.Remove(item);
            this.FileInformationList.Insert(index, item);
        }

        // リストにアイテムを追加する
        public void AddCollection(FileInformation item)
        {
            this.FileInformationList.Remove(item);
            this.FileInformationList.Add(item);
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

        // フィルタを拡張子に変換
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
        #endregion ======= Method; Control Operation ==========================================================================================


        #region ---------- Method: File Operation ------------------------------------------------------------------------------------------
        // 出力処理（非同期）
        public async void OutputBinderProcess()
        {
            if (isCheckSetupError()) return;

            this.IsOutputProcess = true;

            this.AddLog("出力処理を開始します。（ファイル数：" + this.FileInformationList.Count.ToString() + "）");
            this._cancelTokenSource = new CancellationTokenSource();
            this.ProgressValue = 0;
            this.ProgressMax = this.FileInformationList.Count + 2;

            // 作業実行用フォルダの作成 & なければ作成
            string xdwSaveFolder = this.OutputDirectory + "xdw";
            if (!Directory.Exists(xdwSaveFolder)) Directory.CreateDirectory(xdwSaveFolder);

            // バインダーの存在確認 & あれば削除
            string binderPath = this.OutputDirectory + this.OutputFileName + ".xbd";
            if (File.Exists(binderPath))
            {
                try
                {
                    File.Delete(binderPath);
                }
                catch
                {
                    MessageBox.Show("ファイル削除に失敗しました。ファイルを開いている場合は閉じてからもう一度操作してください。");
                    this.IsOutputProcess = false;
                    this.CancelOn();
                    return;
                }
            }


            // バインダーを作成
            int result = this._model.CreateBinder(binderPath);
            if (result != 0) this.AddLog(this._model.ConvertErrorCodeToString(result));

            // DocuWorks文書に印刷
            var documentsPath = await Task.Run(() => this.OutputDocuWorksDocuments(xdwSaveFolder, this._cancelTokenSource.Token));
            int errorDocuments = this.FileInformationList.Count - documentsPath.Count;

            // バインダーに追加
            int errorAddFiles = await Task.Run(() => OutputBinder(documentsPath, binderPath, this.IsXdwSave));

            // 作業用フォルダ削除
            if (!this.IsXdwSave)
                this._model.DeleteFolder(xdwSaveFolder);
            this.ProgressValue++;

            this.IsOutputProcess = false;
            string message = this.CreateCompleteMessage(errorDocuments, errorAddFiles, this._cancelTokenSource.Token);
            this.AddLog(message);
            MessageBox.Show(message);
        }

        // エラーチェック
        private bool isCheckSetupError()
        {
            ErrorCode errorcodeClass = new ErrorCode();

            ErrorCode.eErrorCode error = ErrorCode.eErrorCode.Ok;

            if (this.FileInformationList.Count <= 0) error = ErrorCode.eErrorCode.ErrorNoneOutputFile;
            else if (this.OutputFileName == "") error = ErrorCode.eErrorCode.ErrorOutputFileName;

            if (error != ErrorCode.eErrorCode.Ok)
            {
                MessageBox.Show(errorcodeClass.GetErrorMessage(error));
                return true;
            }
            else return false;
        }

        // バインダーに出力
        public int OutputBinder(List<string> documentsPath, string binderPath, bool isXdwSave)
        {
            int result = 0;

            // Documentをバインダーに格納
            int errorAddFiles = 0;
            result = this._model.InsertDocumentToBinder(documentsPath, binderPath, ref errorAddFiles);
            if (result != 0) this.AddLog(this._model.ConvertErrorCodeToString(result));
            this.ProgressValue++;

            return errorAddFiles;
        }

        // DocuWorks文書印刷
        private List<string> OutputDocuWorksDocuments(string outputFolder, CancellationToken token)
        {
            int result = 0;
            List<string> documentsPath = new List<string>();
            List<string> errorPath = new List<string>();

            string pdfSaveFolder = outputFolder + "\\pdf";
            if (!Directory.Exists(pdfSaveFolder)) Directory.CreateDirectory(pdfSaveFolder);

            // エクセル印刷用エクセルブックを開く
            if (this._model.IsOpenExcel())
            {
                string message = this._model.ConvertErrorCodeToString((int)ErrorCode.eErrorCode.ErrorNoneFile);
                this.AddLog(message + ": " + this._model.GetMacroFilePath());
                return documentsPath;
            }

            // DocuWorks文書印刷
            foreach (var item in this.FileInformationList)
            {
                // キャンセル確認
                if (token.IsCancellationRequested) break;

                string filepath = item.FilePath;
                // エクセルならマクロを介して一度PDF出力する
                if (item.FileType == FileInformation.eFileType.Excel)
                    filepath = this._model.ConvertExcelBookToDocuWorks(pdfSaveFolder, item.FilePath);

                if (File.Exists(filepath))
                {
                    // DocuWorks印刷
                    string documentPath = "";
                    result = this._model.CreateDocuWorksFile(filepath, outputFolder, ref documentPath);
                    if (result == 0)
                    {
                        // ファイルパス保存
                        documentsPath.Add(documentPath);
                    }
                    else
                    {
                        string message = this._model.ConvertErrorCodeToString(result);
                        this.AddLog(message + ": " + item.FilePath);
                    }
                }
                else
                {
                    string message = this._model.ConvertErrorCodeToString((int)ErrorCode.eErrorCode.ErrorNoneFile);
                    this.AddLog(message + ": " + item.FilePath);
                }
                this.ProgressValue++;
            }

            // エクセルを閉じる
            this._model.CloseExcel();
            // Pdf作成場所の削除
            this._model.DeleteFolder(pdfSaveFolder);

            return documentsPath;
        }

        // ファイルパラメータに変換
        public void ConvertToFileInformationList(IEnumerable<string> pathList)
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
            this.AddLog(pathList.Count().ToString() + "ファイル読み込みました。");
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
        #endregion ======= Method: File Operation ==========================================================================================
    }
}
