/*
 * イメージファイルからDocuWorks文書を作る
 *
 *使用方法:
 * xdwsmpl イメージファイル名[出力ファイル名]
 * イメージファイル名: bmp、jpgまたはtifのイメージファイル
 *
 * 出力ファイル: 作られるDocuWorks文書のファイル名
 * 出力ファイルを省略すると、DocuWorks Deskが起動されて、そこに生成される
*/
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using FujiXerox.DocuWorks.Toolkit;

namespace WpfApp2DocuworksTest.DocuWorksOperation
{
    public class DocuworksCreater
    {
        public static int CreateBinder(string path)
        {
            int apiResult = 0;
            string outputPath = path;
            // pathが無ければ
            if(outputPath == "")
            {
                // end
            }
            else
            {
                Xdwapi.XDW_BINDER_INITIAL_DATA initialData = new Xdwapi.XDW_BINDER_INITIAL_DATA();
                apiResult = Xdwapi.XDW_CreateBinder(path, initialData);
            }
            return apiResult;
        }

        public static int CreateDocuWorksFile(string inputPath)
        {
            int apiResult = 0;

            string outputPath = "";
            outputPath += Path.GetDirectoryName(inputPath);
            outputPath += "\\";
            outputPath += Path.GetFileNameWithoutExtension(inputPath);
            outputPath += ".xdw";

            Xdwapi.XDW_CREATE_HANDLE createHandle = new Xdwapi.XDW_CREATE_HANDLE();
            apiResult = Xdwapi.XDW_BeginCreationFromAppFile(inputPath, outputPath, true, ref createHandle);

            return apiResult;
        }

        // kill
        public static int MainOperation(string[] args)
        {
            int api_result = 0;

            if (args.Length != 2 && args.Length != 1)
            {
                TextWriter errorWriter = Console.Error;
                errorWriter.WriteLine("使用方法: xdwsmpl イメージファイル名 [出力ファイル名]");
                return 0;
            }

            string in_path = Path.GetFullPath(args[0]);

            string out_path = null;
            if (args.Length == 2)
            {
                out_path = Path.GetFullPath(args[1]);
            }
            else
            {
                // XDW_GetInformationを利用してディレクトリを取得
                // XDW_GI_DWINPUTPATHで選られるディレクトリにDocuWorks文書を作成し
                // DocuWorks Deskを起動するとDocuWorks Desk上に文書が現れる
                api_result = Xdwapi.XDW_GetInformation(Xdwapi.XDW_GI_DWINPUTPATH, ref out_path);
                if (api_result < 0)
                {
                    print_error(api_result);
                    return 0;
                }
                string fname = Path.GetFileNameWithoutExtension(in_path);
                out_path += "\\";
                out_path += fname;
                out_path += ".xdw";
            }

            // XDW_CreateXdwFromImageFileを利用してイメージからDocuWorks文書を生成
            // イメージの大きさのページサイズとなるようにFitImageにXDW_CREATE_FITを指定
            Xdwapi.XDW_CREATE_OPTION_EX2 option = new Xdwapi.XDW_CREATE_OPTION_EX2();
            option.FitImage = Xdwapi.XDW_CREATE_FIT;

            api_result = Xdwapi.XDW_CreateXdwFromImageFile(in_path, out_path, option);
            if (api_result < 0)
            {
                print_error(api_result);
                return 0;
            }

            if (args.Length == 2)
            {
                // XDW_GetInformationを利用してDocuWorks Deskプログラムのパスを取得
                string desk_path = null;
                api_result = Xdwapi.XDW_GetInformation(Xdwapi.XDW_GI_DWDESKPATH, ref desk_path);
                if (api_result < 0)
                {
                    print_error(api_result);
                    return 0;
                }

                Process.Start(desk_path);
            }

            return 1;
        }

        static void print_error(int code)
        {
            TextWriter errorWriter = Console.Error;
            switch (code)
            {
                case Xdwapi.XDW_E_NOT_INSTALLED:
                    errorWriter.WriteLine("DocuWorksがインストールされていません。");
                    break;
                case Xdwapi.XDW_E_FILE_NOT_FOUND:
                    errorWriter.WriteLine("指定されたファイルが見つかりません。");
                    break;
                case Xdwapi.XDW_E_FILE_EXISTS:
                    errorWriter.WriteLine("指定されたファイルはすでに存在します。");
                    break;
                case Xdwapi.XDW_E_ACCESSDENIED:
                case Xdwapi.XDW_E_INVALID_ACCESS:
                case Xdwapi.XDW_E_INVALID_NAME:
                case Xdwapi.XDW_E_BAD_NETPATH:
                    errorWriter.WriteLine("指定されたファイルを開くことができません。");
                    break;
                case Xdwapi.XDW_E_BAD_FORMAT:
                    errorWriter.WriteLine("指定されたファイルは正しいフォーマットではありません。");
                    break;
                default:
                    errorWriter.WriteLine("エラーが発生しました。");
                    break;
            }
        }
    }
}
