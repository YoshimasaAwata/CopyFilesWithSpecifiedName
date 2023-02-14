﻿using System;
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
    /// ErrorDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class ErrorDialog : UserControl
    {
        /// <summary>
        /// ダイアログのタイプ
        /// </summary>
        public enum Type
        {
            /// <value>エラーメッセージ用ダイアログ</value>
            Error,
            /// <value>警告メッセージ用ダイアログ</value>
            Warning,
            /// <value>情報メッセージ用ダイアログ</value>
            Info,
        }

        /// <param name="message">ダイアログに表示するメッセージ</param>
        /// <param name="type">ダイアログのタイプ</param>
        public ErrorDialog(string message, Type type = Type.Error)
        {
            InitializeComponent();

            Message.Text = message;

            if (type == Type.Warning)
            {
                WarningPanel.Visibility = Visibility.Visible;
            }
            else if (type == Type.Info)
            {
                InfoPanel.Visibility = Visibility.Visible;
            }
            else
            {
                ErrorPanel.Visibility = Visibility.Visible;
            }
        }
    }
}
