using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public partial class MainWindow : Window
    {
        private FileList fileList = new FileList();     // リストボックスに表示するファイル名のリスト

        public MainWindow()
        {
            InitializeComponent();

            FromListBox.ItemsSource = fileList.FileNameList;
            ToListBox.ItemsSource = fileList.FileNameList;
            FileNameTextBox.Text = fileList.BaseFileName;
        }

        /// <summary>
        /// ファイル選択ダイアログボックスを表示してコピー元フォルダを選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FromButton_Click(object sender, RoutedEventArgs e)
        {
            using (var openFolderDialog = new CommonOpenFileDialog()
            {
                Title = "コピー元フォルダを選択してください",
                IsFolderPicker = true,
            })
            {
                if (openFolderDialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    var sourceDir = openFolderDialog.FileName;
                    var rc = fileList.SetSourceDir(sourceDir);
                    if (rc == FileList.Code.NG)
                    {
                        MessageBox.Show(fileList.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        FromTextBox.Text = sourceDir;
                        // コピーするファイルがない場合もCopyボタンは無効
                        CopyButton.IsEnabled = ((rc == FileList.Code.OK) && (fileList.FileNameList.Count > 0));
                    }
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
                    fileList.TargetDir = targetDir;
                }
            }
        }

        /// <summary>
        /// 実際にファイルのコピーを行う
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            CopyButton.IsEnabled = false;
            var rc = fileList.CopyFiles();
            if (rc < 0)
            {
                MessageBox.Show(fileList.Message);
            }
            else
            {
                MessageBox.Show("コピーしました。");
            }
            CopyButton.IsEnabled = true;
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
        private void FileNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            fileList.BaseFileName = FileNameTextBox.Text;
            ToListBox.Items.Refresh();
        }
    }
}
