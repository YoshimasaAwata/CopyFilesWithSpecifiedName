using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CopyFilesWithSpecifiedName
{

    /// <summary>
    /// コピー元およびコピー先のファイル名のリストの管理<br/>
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
            /// <value>キャンセル</value>
            Cancel,
        }

        /// <value>フィルタリング用拡張子を複数指定する際の区切り文字</value>
        private static readonly char[] s_delimiter = { ',', ' ', '.', ';', ':' };

        /// <value>FFmpegを使用できるかどうか</value>
        public bool FFmpeg { get; set; } = false;
        /// <value>コピー元およびコピー先のファイル名のリスト</value>
        public ObservableCollection<FileNames> FileNameList { get; } = new ObservableCollection<FileNames>();
        /// <value>エラー等のメッセージ</value>
        public string Message { get; private set; } = "";
        /// <value>ファイルのコピー先フォルダ名</value>
        public string TargetDir { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        /// <value>コピー先ファイル名の共通部分</value>
        public string BaseFileName { get; private set; } = "{0:d1}";
        /// <value>フィルタリング用拡張子のリスト</value>
        private List<string>? _extensions = null;
        /// <value>ファイルコピーキャンセル用</value>
        private CancellationTokenSource? _tokenSource = null;
        /// <value>ロック用オブジェクト</value>
        private readonly object _balanceLock = new object();
        /// <value>連番の最小桁数</value>
        public int Digit { get; private set; } = 1;
        /// <value>連番のスタートの値</value>
        public int StartNum { get; private set; } = 0;

        public FileList()
        {
            var info = new ProcessStartInfo()
            {
                FileName = "ffplay",
                Arguments = "-version",
                CreateNoWindow = true,
            };
            var process = Process.Start(info);
            if (process != null)
            {
                process.WaitForExit();
                if (process.ExitCode == 0)
                {
                    FFmpeg = true;
                }
            }
        }

        /// <summary>
        /// 指定のファイル名を"FileNameList"に追加<br/>
        /// 指定により隠しファイルやシステムファイルは除外する
        /// </summary>
        /// <param name="file">追加するファイル名</param>
        /// <param name="exclude">隠しファイルやシステムファイルを除外するかどうか</param>
        protected void AddFromFile(string file, bool? exclude)
        {
            if ((exclude != null) && (exclude == false))
            {
                FileNameList.Add(new FileNames() { FromFile = file });
            }
            else    // exclude == true
            {
                var attr = File.GetAttributes(file);
                if ((attr & (FileAttributes.Hidden | FileAttributes.System)) == 0)
                {
                    FileNameList.Add(new FileNames() { FromFile = file });
                }
            }
        }

        /// <summary>
        /// コピー元ファイルをセット<br/>
        /// 同時にコピーするファイル名をリストに登録
        /// </summary>
        /// <param name="dir">ファイルのコピー元フォルダ名</param>
        /// <param name="exclude">隠しファイルやシステムファイルを除外するかどうか</param>
        /// <returns>処理結果</returns>
        public Code AddSourceFiles(IEnumerable<string> files, bool? exclude)
        {
            Code result = Code.OK;

            try
            {
                if ((_extensions != null) && (_extensions.Count > 0))
                {
                    foreach (var file in files)
                    {
                        var ext = Path.GetExtension(file).Substring(1); // 最初の'.'を除く
                        if (_extensions.Contains(ext))
                        {
                            AddFromFile(file, exclude);
                        }
                    }
                }
                else
                {
                    foreach (var file in files)
                    {
                        AddFromFile(file, exclude);
                    }
                }

                MakeToFilesList();
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
            int num = StartNum;
            foreach (var file in FileNameList)
            {
                var ext = Path.GetExtension(file.FromFile);
                var newFileName = String.Format(BaseFileName, num);
                file.ToFile = newFileName + ext;
                num++;
            }
        }

        /// <summary>
        /// "FileNameList"にリストアップされたコピー元ファイル名とコピー先ファイル名、コピー先フォルダ名を使いファイルをコピー
        /// </summary>
        /// <param name="progress">プログレスバーダイアログ</param>
        /// <returns>処理結果</returns>
        public async Task<Code> CopyFiles(FileCopyProgress progress)
        {
            Code result = Code.OK;
            int fileCount = 0;

            // コピー先フォルダの指定がない場合はHOMEPATHに指定されたフォルダにコピー
            var dir = (TargetDir == "") ? Environment.GetEnvironmentVariable("HOMEPATH") : TargetDir;

            lock (_balanceLock)
            {
                _tokenSource = new CancellationTokenSource();
            }

            foreach (var file in FileNameList)
            {
                var targetFileName = Path.Join(dir, file.ToFile);

                progress.SetFileNameProgress(Path.GetFileName(file.FromFile), (++fileCount * 100 / FileNameList.Count));

                try
                {
                    using (var source = File.OpenRead(file.FromFile))
                    {
                        using (var target = File.Open(targetFileName, FileMode.CreateNew))
                        {
                            await source.CopyToAsync(target, _tokenSource.Token).ConfigureAwait(false);
                        }
                    }
                }
                catch (IOException)
                {
                    // ファイルが既に存在する等のエラーはそのまま続行する
                    continue;
                }
                catch (Exception e)
                {
                    Message = e.Message;
                    result = _tokenSource.IsCancellationRequested ? Code.Cancel : Code.NG;
                    break;
                }
            }

            lock (_balanceLock)
            {
                _tokenSource.Dispose();
                _tokenSource = null;
            }

            return result;
        }

        /// <summary>
        /// コピー元ファイルのフィルタリング用拡張子のリストを作成<br/>
        /// 拡張子は区切り文字',', ' ', '.', ';', ':'を用いて複数指定できる
        /// </summary>
        /// <param name="ext">拡張子を記述した文字列</param>
        public void SetExtensions(string ext)
        {
            var extList = ext.Split(s_delimiter, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            _extensions = (extList.Length > 0) ? new List<string>(extList) : null;
        }

        /// <summary>
        /// 拡張子のリストが要素を持っているか確認
        /// </summary>
        /// <returns>要素を持っていればtrue</returns>
        public bool HasExtensions()
        {
            return ((_extensions != null) && (_extensions.Count > 0));
        }

        /// <summary>
        /// "FileNameList"から指定のインデックスの要素を削除<br/>
        /// 削除後にコピー先ファイル名を付けなおす
        /// </summary>
        /// <param name="index">削除する要素のインデックス</param>
        public void DeleteElement(Int32 index)
        {
            if ((index >= 0) && (index < FileNameList.Count))
            {
                FileNameList.RemoveAt(index);
                MakeToFilesList();
            }
        }

        /// <summary>
        /// "FileNameList"の全要素を削除
        /// </summary>
        public void ClearElements()
        {
            FileNameList.Clear();
        }

        /// <summary>
        /// "FileNameList"の指定のインデックスの要素を上に移動<br/>
        /// 移動後にコピー先ファイル名を付けなおす
        /// </summary>
        /// <remarks>
        /// 先頭の要素は移動できないので何もしない
        /// </remarks>
        /// <param name="index">移動する要素のインデックス</param>
        public void UpElement(Int32 index)
        {
            if ((index > 0) && (index < FileNameList.Count))
            {
                FileNameList.Move(index, (index - 1));
                MakeToFilesList();
            }
        }

        /// <summary>
        /// "FileNameList"の指定のインデックスの要素を下に移動<br/>
        /// 移動後にコピー先ファイル名を付けなおす
        /// </summary>
        /// <remarks>
        /// 最後の要素は移動できないので何もしない
        /// </remarks>
        /// <param name="index">移動する要素のインデックス</param>
        public void DownElement(Int32 index)
        {
            if ((index >= 0) && (index < (FileNameList.Count - 1)))
            {
                FileNameList.Move(index, (index + 1));
                MakeToFilesList();
            }
        }

        /// <summary>
        /// ファイルのコピーを中断
        /// </summary>
        public void CancelCopy()
        {
            lock (_balanceLock)
            {
                _tokenSource?.Cancel();
            }
        }

        /// <summary>
        /// 連番の開始番号をセット<br/>
        /// コピー先ファイル名は付けなおし
        /// </summary>
        /// <param name="num">連番の開始番号</param>
        public void SetStartNum(int num)
        {
            StartNum = num;
            MakeToFilesList();
        }

        /// <summary>
        /// 連番の桁数をセット<br/>
        /// コピー先ファイル名は付けなおし
        /// </summary>
        /// <param name="num">連番の桁数</param>
        public void SetDigit(int num)
        {
            Digit = num;

            var reg = new Regex("{0:d[0-9]}");
            BaseFileName = reg.Replace(BaseFileName, $"{{0:d{Digit}}}");
            MakeToFilesList();
        }

        /// <summary>
        /// コピー先の共通ファイル名をセット
        /// </summary>
        /// <remarks>
        /// 共通ファイル名には?もしくは文末を連番と置き換えるためのフォーマット部分を追加
        /// </remarks>
        /// <param name="fileName"></param>
        public void SetBaseFileName(string fileName)
        {
            var index = fileName.IndexOf('?');
            if (index >= 0)
            {
                fileName = fileName.Replace("?", $"{{0:d{Digit}}}");
            }
            else
            {
                fileName += $"{{0:d{Digit}}}";
            }
            BaseFileName = fileName;
            MakeToFilesList();
        }

        /// <summary>
        /// 指定した拡張子以外のコピー元ファイルを削除
        /// </summary>
        public void FilteringFromList()
        {
            if ((_extensions != null) && (_extensions.Count > 0))
            {
                var removeList = new List<FileNames>();
                foreach (var file in FileNameList)
                {
                    var ext = Path.GetExtension(file.FromFile).Substring(1);    // 先頭の'.'は除く
                    var contain = _extensions.Contains(ext);
                    if (!contain)
                    {
                        removeList.Add(file);
                    }
                }
                foreach (var file in removeList)
                {
                    FileNameList.Remove(file);
                }
                MakeToFilesList();
            }
        }

        public async Task<bool> PlayWithFFmpeg(int index)
        {
            bool rc = false;
            var playFile = FileNameList[index].FromFile;
            if (playFile != null)
            {
                var info = new ProcessStartInfo()
                {
                    FileName = "ffplay",
                    Arguments = $"\"{playFile}\"",
                    CreateNoWindow = true,
                };
                var process = Process.Start(info);
                if (process != null)
                {
                    await process.WaitForExitAsync();
                    rc = (process.ExitCode == 0);
                }
            }
            return rc;
        }
    }
}
