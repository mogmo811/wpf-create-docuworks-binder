using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Binder
{
    public class ErrorCode
    {
        // エラーコード
        public enum eErrorCode
        {
            Ok = 0,
            ErrorOutputFileName,
            ErrorNoneOutputFile,
            ErrorPath,
            ErrorNoneFile,

            ErrorXdwCreateBinder,
            ErrorXdwApiBeginCreateXdw,
            ErrorXdwGetStatusCreateXdw,

            ErrorOpenBinder,
            ErrorAddBinder,
            ErrorSaveBinder
        }

        // エラーメッセージの表示
        public string GetErrorMessage(eErrorCode errorcode)
        {
            string message = "";
            switch (errorcode)
            {
                case eErrorCode.ErrorPath:
                    message = Properties.Resources.ErrorMessagePath; break;
                case eErrorCode.ErrorOutputFileName:
                    message = Properties.Resources.ErrorMessageOutputFileName; break;
                case eErrorCode.ErrorNoneOutputFile:
                    message = Properties.Resources.ErrorMessageNoneOutputFile; break;
                case eErrorCode.ErrorXdwCreateBinder:
                    message = "[エラー] バインダー作成に失敗しました。"; break;
                case eErrorCode.ErrorXdwApiBeginCreateXdw:
                    message = "[エラー] XDWAPI_dotNET DocuWorks文書変換開始に失敗しました。"; break;
                case eErrorCode.ErrorXdwGetStatusCreateXdw:
                    message = "[エラー] XDWAPI_dotNET DocuWorks文書変換状態取得に失敗しました。"; break;
                case eErrorCode.ErrorNoneFile:
                    message = "[エラー] ファイルが見つかりません。"; break;
                case eErrorCode.ErrorOpenBinder:
                    message = "[エラー] バインダーを開けませんでした"; break;
                case eErrorCode.ErrorAddBinder:
                    message = "[エラー] バインダー文書追加に失敗しました。"; break;
                case eErrorCode.ErrorSaveBinder:
                    message = "[エラー] バインダー保存に失敗しました。"; break;
                default: message = Properties.Resources.ErrorMessageOther; break;
            }
            return message;
        }
    }
}
