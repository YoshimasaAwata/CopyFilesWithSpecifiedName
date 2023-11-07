using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using MaterialDesignThemes.Wpf;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CopyFilesWithSpecifiedName
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        /// <value>?を除くファイル名の禁則文字のリスト</value>
        private static readonly char[] s_forbidden = { '/', '<', '>', '\\', ':', '*', '|', '"' };
        /// <value>リストボックスに表示するファイル名のリスト</value>
        private FileList _fileList = new FileList();
        /// <value>Fromリストボックス内のScrollViewer</value>
        private ScrollViewer? _fromScrollViewer;
        /// <value>Toリストボックス内のScrollViewer</value>
        private ScrollViewer? _toScrollViewer;

        public MainWindow()
        {
            InitializeComponent();

            FromListBox.ItemsSource = _fileList.FileNameList;
            ToListBox.ItemsSource = _fileList.FileNameList;
            FileNameTextBox.Text = "new-file-name";
            ToTextBox.Text = _fileList.TargetDir;
        }

        /// <summary>
        /// ファイル選択ダイアログボックスを表示してコピー元フォルダを選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            bool excludeHiddenFIles = (ExcludeCheck.IsChecked == null) ? false : (!(bool)ExcludeCheck.IsChecked);
            using (var openFolderDialog = new CommonOpenFileDialog()
            {
                Title = "コピー元ファイルを選択してください",
                Multiselect = true,
                ShowHiddenItems = excludeHiddenFIles,
            })
            {
                if (openFolderDialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    var sourceFiles = openFolderDialog.FileNames;
                    var rc = _fileList.AddSourceFiles(sourceFiles, excludeHiddenFIles);
                    if (rc == FileList.Code.NG)
                    {
                        await DialogHost.Show(new ErrorDialog(_fileList.Message, ErrorDialog.Type.Error));
                    }
                    // コピーするファイルがない場合はCopyボタンやクリアボタン、フィルタリングボタンが無効
                    var enable = (_fileList.FileNameList.Count > 0);
                    CopyButton.IsEnabled = enable;
                    ClearButton.IsEnabled = enable;
                    FilterButton.IsEnabled = _fileList.HasExtensions() && enable;
                }
            }
        }

        /// <summary>
        /// ファイル選択ダイアログボックスを表示してコピー先フォルダを選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToButton_Click(object sender, RoutedEventArgs e)
        {
            using (var openFolderDialog = new CommonOpenFileDialog()
            {
                Title = "コピー先のフォルダを選択してください",
                IsFolderPicker = true,
            })
            {
                if (openFolderDialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    var targetDir = openFolderDialog.FileName;
                    ToTextBox.Text = targetDir;
                    _fileList.TargetDir = targetDir;
                }
            }
        }

        /// <summary>
        /// 実際にファイルのコピーを行う
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            OverallGrid.IsEnabled = false;

            var progress = new FileCopyProgress();
            var copyTask = _fileList.CopyFiles(progress);
            var progressTask = DialogHost.Show(progress);

            var taskDone = await Task.WhenAny(copyTask, progressTask);

            if (taskDone == progressTask)
            {
                _fileList.CancelCopy();
                await copyTask;
                await DialogHost.Show(new ErrorDialog("キャンセルされました。", ErrorDialog.Type.Warning));
            }
            else
            {
                DialogHost.Close(null);
                await progressTask;

                var rc = copyTask.Result;
                if (rc == FileList.Code.NG)
                {
                    await DialogHost.Show(new ErrorDialog(_fileList.Message, ErrorDialog.Type.Error));
                }
                else
                {
                    await DialogHost.Show(new ErrorDialog("コピーしました。", ErrorDialog.Type.Info));
                }
            }

            OverallGrid.IsEnabled = true;
        }

        /// <summary>
        /// アプリケーションを終了
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        /// <summary>
        /// 共通ファイル名をアップデートする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void FileNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string fileName = FileNameTextBox.Text;
            if (fileName.IndexOfAny(s_forbidden) < 0)
            {
                _fileList.SetBaseFileName(FileNameTextBox.Text);
                ToListBox.Items.Refresh();
            }
            else
            {
                await DialogHost.Show(new ErrorDialog("ファイル名に/<>\\:*|\"は使用できません。", ErrorDialog.Type.Warning));
                FileNameTextBox.Text = _fileList.BaseFileName;
            }
        }

        /// <summary>
        /// AboutBoxを表示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            var about = new AboutBox();
            about.ShowDialog();
        }

        /// <summary>
        /// コピー元ファイルのフィルタリング用拡張子のリストを作成<br/>
        /// 拡張子は区切り文字',', ' ', '.', ';', ':'を用いて複数指定できる
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExtensionTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _fileList.SetExtensions(ExtensionTextBox.Text);
            FilterButton.IsEnabled = (_fileList.HasExtensions() && (_fileList.FileNameList.Count > 0)); 
        }

        /// <summary>
        /// "FromListBox"を選択した際にUpボタンとDownボタン、Deleteボタンの有効化/無効化を行う<br/>
        /// 更に"ToListBox"の選択を"FromListBox"に反映
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FromListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var enable = (FromListBox.SelectedIndex >= 0);

            UpButton.IsEnabled = enable;
            DownButton.IsEnabled = enable;
            DeleteButton.IsEnabled = enable;

            if (FromListBox.SelectedIndex == 0)
            {
                UpButton.IsEnabled = false;
            }

            if (FromListBox.SelectedIndex == (_fileList.FileNameList.Count - 1))
            {
                DownButton.IsEnabled = false;
            }

            ToListBox.SelectedIndex = FromListBox.SelectedIndex;
        }

        /// <summary>
        /// "ToListBox"の選択を"FromListBox"に反映
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FromListBox.SelectedIndex = ToListBox.SelectedIndex;
        }

        /// <summary>
        /// "FromListBox"から選択したリストアイテムを削除<br/>
        /// 削除後にコピー先ファイル名を付けなおし、リスト表示をアップデート
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            _fileList.DeleteElement(FromListBox.SelectedIndex);

            FromListBox.Items.Refresh();
            ToListBox.Items.Refresh();

            if (_fileList.FileNameList.Count <= 0)
            {
                DeleteButton.IsEnabled = false;
                ClearButton.IsEnabled = false;
                FilterButton.IsEnabled = false;
            }
        }

        /// <summary>
        /// "FromListBox"から全リストアイテムを削除<br/>
        /// 削除後にリスト表示をアップデート
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            _fileList.ClearElements();

            FromListBox.Items.Refresh();
            ToListBox.Items.Refresh();

            DeleteButton.IsEnabled = false;
            ClearButton.IsEnabled = false;
            CopyButton.IsEnabled = false;
            UpButton.IsEnabled = false;
            DownButton.IsEnabled = false;
            FilterButton.IsEnabled = false;
        }

        /// <summary>
        /// "FromListBox"から選択したリストアイテムを上に移動<br/>
        /// 移動後にコピー先ファイル名を付けなおし、リスト表示をアップデート
        /// </summary>
        /// <remarks>
        /// 移動後のリストアイテムの位置によりボタンの有効化/無効化を指定
        /// </remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpButton_Click(object sender, RoutedEventArgs e)
        {
            _fileList.UpElement(FromListBox.SelectedIndex);

            FromListBox.Items.Refresh();
            ToListBox.Items.Refresh();

            UpButton.IsEnabled = (FromListBox.SelectedIndex > 0);
            DownButton.IsEnabled = (FromListBox.SelectedIndex < (_fileList.FileNameList.Count - 1));
        }

        /// <summary>
        /// "FromListBox"から選択したリストアイテムを下に移動<br/>
        /// 移動後にコピー先ファイル名を付けなおし、リスト表示をアップデート
        /// </summary>
        /// <remarks>
        /// 移動後のリストアイテムの位置によりボタンの有効化/無効化を指定
        /// </remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DownButton_Click(object sender, RoutedEventArgs e)
        {
            _fileList.DownElement(FromListBox.SelectedIndex);

            FromListBox.Items.Refresh();
            ToListBox.Items.Refresh();

            UpButton.IsEnabled = (FromListBox.SelectedIndex > 0);
            DownButton.IsEnabled = (FromListBox.SelectedIndex < (_fileList.FileNameList.Count - 1));
        }

        /// <summary>
        /// 渡されたコントロールの指定のタイプの子コントロールを取得
        /// </summary>
        /// <typeparam name="T">取得する子コントロールのタイプ</typeparam>
        /// <param name="dependencyObject">親コントロール</param>
        /// <returns>指定のタイプの子コントロールもしくはnull</returns>
        private T? GetDependencyObject<T>(DependencyObject dependencyObject) where T : DependencyObject
        {
            T? obj = null;
            if (dependencyObject != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(dependencyObject); i++)
                {
                    var child = VisualTreeHelper.GetChild(dependencyObject, i);
                    obj = (child as T) ?? GetDependencyObject<T>(child);
                    if (obj != null)
                    {
                        break;
                    }
                }
            }
            return obj;
        }

        /// <summary>
        /// "FromListBox"内のScrollViewerがスクロールした際のイベントハンドラ<br/>
        /// "FromListBox"のスクロール量を"ToListBox"に反映
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FromListBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if ((_toScrollViewer != null) && (_fromScrollViewer != null))
            {
                _toScrollViewer.ScrollToVerticalOffset(_fromScrollViewer.VerticalOffset);
            }
        }

        /// <summary>
        /// "ToListBox"内のScrollViewerがスクロールした際のイベントハンドラ<br/>
        /// "ToListBox"のスクロール量を"FromListBox"に反映
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToListBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if ((_fromScrollViewer != null) && (_toScrollViewer != null))
            {
                _fromScrollViewer.ScrollToVerticalOffset(_toScrollViewer.VerticalOffset);
            }
        }

        /// <summary>
        /// "FromListBox"の描画終了時のイベントハンドラ<br/>
        /// "FromListBox"内のScrollViewrを取得すると共にScrollChangedイベントハンドラを追加
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FromListBox_Loaded(object sender, RoutedEventArgs e)
        {
            _fromScrollViewer = GetDependencyObject<ScrollViewer>(FromListBox);
            if (_fromScrollViewer != null)
            {
                _fromScrollViewer.ScrollChanged += new ScrollChangedEventHandler(FromListBox_ScrollChanged);
            }
        }

        /// <summary>
        /// "ToListBox"の描画終了時のイベントハンドラ<br/>
        /// "ToListBox"内のScrollViewrを取得すると共にScrollChangedイベントハンドラを追加
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToListBox_Loaded(object sender, RoutedEventArgs e)
        {
            _toScrollViewer = GetDependencyObject<ScrollViewer>(ToListBox);
            if (_toScrollViewer != null)
            {
                _toScrollViewer.ScrollChanged += new ScrollChangedEventHandler(ToListBox_ScrollChanged);
            }
        }

        /// <summary>
        /// 数値のみ許可
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartNumTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !new Regex("[0-9]").IsMatch(e.Text);
        }

        /// <summary>
        /// 貼り付けを許可しない
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartNumTextBox_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Paste)
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// 連番をスタートする値の変更<br/>
        /// 変更後にコピー先ファイル名を付けなおし、リスト表示をアップデート
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartNumTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var num = (StartNumTextBox.Text == "") ? 0 : int.Parse(StartNumTextBox.Text);
            _fileList.SetStartNum(num);

            ToListBox?.Items.Refresh();
        }

        /// <summary>
        /// 桁数の選択を変更<br/>
        /// 変更後にコピー先ファイル名を付けなおし、リスト表示をアップデート
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DigitsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _fileList.SetDigit(DigitsComboBox.SelectedIndex + 1);

            ToListBox?.Items.Refresh();
        }

        /// <summary>
        /// 拡張子でコピー元リストのフィルタリング
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            _fileList.FilteringFromList();

            FromListBox?.Items.Refresh();
            ToListBox?.Items.Refresh();
        }
    }
}
