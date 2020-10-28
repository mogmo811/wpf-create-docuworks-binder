
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Binder.Operation;
using Binder.Parameter;
using System.Threading;

namespace Binder.MyWindow.Models
{
    public class MainWindowModel
    {
        PdfConverter _pdfConverter = new PdfConverter();
        FileOperation _fileOperation = new FileOperation();
        ErrorCode _errorCode = new ErrorCode();
        const string _excelExtension = ".xlsx";
        const string _excelMacroExtension = ".xlsm";
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindowModel()
        {
        }

        #region メソッド
        public int GetExportProgress()
        {
            return _pdfConverter.ExportProgress;
        }

        public async Task<bool> FunctionExportAsync(ExportSettings exportSettings, List<string> inputFilePaths, CancellationToken token)
        {
            if (exportSettings == null) return false;
            if (inputFilePaths.Count == 0) return false;

            List<string> outputFiles = new List<string>();
            switch (exportSettings.ExportFormat)
            {
                case ExportSettings.eExportFormat.PdfBinder:
                    string tmpFolder = exportSettings.OutputFolderPath + "tmp\\";
                    outputFiles = await FunctionExportToPdfFileAsync(inputFilePaths, tmpFolder, token);
                    PdfConverter.CombinePdfFiles(outputFiles, exportSettings.OutputFolderPath + exportSettings.OutputFileName);
                    if (Directory.Exists(tmpFolder))
                        Directory.Delete(tmpFolder, true);
                    break;
                case ExportSettings.eExportFormat.PdfFile:
                    outputFiles = await FunctionExportToPdfFileAsync(inputFilePaths, exportSettings.OutputFolderPath, token);
                    break;
                case ExportSettings.eExportFormat.DocuWorksFile:
                case ExportSettings.eExportFormat.DocuWorksBinder:
                    // TODO
                    break;
                default: break;
            }
            return true;
        }

        public async Task<List<string>> FunctionExportToPdfFileAsync(List<string> inputFilePaths, string outputFolderPath, CancellationToken token)
        {
            if (String.IsNullOrEmpty(outputFolderPath)) return new List<string>();
            if (inputFilePaths.Count == 0) return new List<string>();

            //出力フォルダの作成
            if (Directory.Exists(outputFolderPath) == false)
                Directory.CreateDirectory(outputFolderPath);

            // PDF変換 
            List<string> list = await _pdfConverter.ExportFilesToPdfFilesAsync(outputFolderPath, inputFilePaths, token);

            return list;
        }
                
        /// <summary>
        /// 除外リストを取得する
        /// </summary>
        /// <returns>除外ファイルの文字列</returns>
        public string GetNotTargetFileName()
        {
            return this._fileOperation.GetNotTargetFileName();
        }

        #endregion
    }
}
