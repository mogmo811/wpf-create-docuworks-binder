
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Binder.Operation;
using static Binder.Operation.FileOperation;

namespace Binder
{
    public class MainWindowModel
    {        
        // Field
        FileOperation _fileOperation = new FileOperation();
        ErrorCode _errorCode = new ErrorCode();
        
        // Construction
        public MainWindowModel()
        {
        }


        #region ---------- Method ------------------------------------------------------------------------------------------
        public string ConvertErrorCodeToString(int errorcode)
        {
            return this._errorCode.GetErrorMessage((ErrorCode.eErrorCode)errorcode);
        }

        // エクセル印刷用マクロのファイルパスを取得
        public string GetMacroFilePath()
        {
            return this._fileOperation.GetMacroFilePath();
        }

        // エクセルを開く
        public bool IsOpenExcel()
        {
            return this._fileOperation.IsOpenExcel();
        }

        // エクセルを閉じる
        public void CloseExcel()
        {
            this._fileOperation.CloseExcel();
            return;
        }

        // 除外リストを編集
        public string GetNotTargetFileName()
        {
            return this._fileOperation.GetNotTargetFileName();
        }

        // フォルダ削除
        public void DeleteFolder(string folderPath)
        {
            this._fileOperation.DeleteFolder(folderPath);
        }

        // エクセルシートのPDF出力マクロ実行
        public string ConvertExcelBookToDocuWorks(string outputFolder, string filepath)
        {
            return this._fileOperation.ConvertExcelBookToPDF(outputFolder, filepath);
        }

        // バインダー作成
        public int CreateBinder(string createPath)
        {
            return this._fileOperation.CreateBinder(createPath);
        }

        // DocuWorksファイル作成
        public int CreateDocuWorksFile(string inputPath, string outputFolder, ref string outputPath)
        {
            outputPath = "";
            return this._fileOperation.CreateDocuWorksFile(inputPath, outputFolder, ref outputPath);
        }

        // バインダーに追加
        public int InsertDocumentToBinder(List<string> documentsPath, string binderPath, ref int errorAddFiles)
        {

            return this._fileOperation.InsertDocumentToBinder(documentsPath, binderPath, ref errorAddFiles);
        }
        #endregion ======= Method ==========================================================================================

    }
}
