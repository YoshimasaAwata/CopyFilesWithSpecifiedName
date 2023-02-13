using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CopyFilesWithSpecifiedName
{

    /// <summary>
    /// コピー元およびコピー先のファイル名のリストの管理</br>
    /// 実際のファイルのコピーも行う
    /// </summary>
    internal class FileList
    {
        /// <summary>
        /// コピー元およびコピー先のファイル名の対
        /// </summary>
        public class FileNames
        {
            /// <value>コピー元ファイル名</value>
            public string FromFile { get; set; } = "";
            /// <value>コピー先ファイル名</value>
            public string ToFile { get; set; } = "";
        }

        /// <summary>
        /// メソッド実行結果
        /// </summary>
        public enum Code
        {
            /// <value>成功</value>
            OK,
            /// <value>失敗</value>
            NG,
        }

        /// <value>フィルタリング用拡張子を複数指定する際の区切り文字</value>
        private static readonly char[] Delimiter = { ',', ' ', '.', ';', ':' };

        /// <value>コピー元およびコピー先のファイル名のリスト</value>
        public ObservableCollection<FileNames> FileNameList { get; } = new ObservableCollection<FileNames>();
        /// <value>エラー等のメッセージ</value>
        public string Message { get; private set; } = "";
        /// <value>ファイルのコピー先フォルダ名</value>
        public string TargetDir { get; set; } = "";
        /// <value>ファイルのコピー元フォルダ名</value>
        public string SourceDir { get; private set; } = "";
        private string baseFileName = "new-file-name";
        /// <value>コピー先ファイル名の共通部分</value>
        public string BaseFileName 
        { 
            get { return baseFileName; } 
            set 
            { 
                baseFileName = value;
                this.MakeToFilesList();     // 変更の度にファイル名リストを更新
            } 
        }
        /// <value>フィルタリング用拡張子のリスト</value>
        private List<string>? extensions = null;

        /// <summary>
        /// ファイルのコピー元フォルダをセット</br>
        /// 同時にコピーするファイル名をリストに登録
        /// </summary>
        /// <param name="dir">ファイルのコピー元フォルダ名</param>
        /// <returns>処理結果</returns>
        public Code SetSourceDir(string dir)
        {
            Code result = Code.OK;

            try
            {
                var files = Directory.GetFiles(dir);
                SourceDir = dir;
                FileNameList.Clear();

                if ((extensions != null) && (extensions.Count > 0))
                {
                    foreach (var file in files)
                    {
                        var ext = Path.GetExtension(file).Substring(1); // 最初の'.'を除く
                        if (extensions.Contains(ext))
                        {
                            FileNameList.Add(new FileNames() { FromFile = file });
                        }
                    }
                }
                else
                {
                    foreach (var file in files)
                    {
                        FileNameList.Add(new FileNames() { FromFile = file });
                    }
                }

                this.MakeToFilesList();
            }
            catch (Exception e)
            {
                Message = e.Message;
                result = Code.NG;
            }

            return result;
        }

        /// <summary>
        /// "FileNameList"内にセットしたコピー元ファイル名と"baseFileName"を元に連番を付加してコピー先ファイル名を作成
        /// </summary>
        protected void MakeToFilesList()
        {
            int num = 0;
            foreach (var file in FileNameList)
            {
                var ext = Path.GetExtension(file.FromFile);
                var newFileName = $"{baseFileName}{num++:d3}{ext}";
                file.ToFile = newFileName;
            }
        }

        /// <summary>
        /// "FileNameList"にリストアップされたコピー元ファイル名とコピー先ファイル名、コピー先フォルダ名を使いファイルをコピー
        /// </summary>
        /// <returns>処理結果</returns>
        public Code CopyFiles()
        {
            Code result = Code.OK;

            // コピー先フォルダの指定がない場合はコピー元フォルダにコピー
            var dir = (TargetDir == "") ? SourceDir : TargetDir;
            
            foreach (var file in FileNameList)
            {
                var targetFileName = Path.Join(dir, file.ToFile);

                try
                {
                    File.Copy(file.FromFile, targetFileName);
                }
                catch (IOException)
                {
                    // ファイルが既に存在する等のエラーはそのまま続行する
                    continue;
                }
                catch (Exception e)
                {
                    Message = e.Message;
                    result = Code.NG;
                    break;
                }
            }
 
            return result;
        }

        /// <summary>
        /// コピー元ファイルのフィルタリング用拡張子のリストを作成</br>
        /// 拡張子は区切り文字',', ' ', '.', ';', ':'を用いて複数指定できる
        /// </summary>
        /// <param name="ext">拡張子を記述した文字列</param>
        public void SetExtensions(string ext)
        {
            var extList = ext.Split(Delimiter, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            extensions = (extList.Length > 0) ? new List<string>(extList) : null;
        }
    }
}
