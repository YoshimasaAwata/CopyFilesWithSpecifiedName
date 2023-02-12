# CopyFilesWithSpecifiedName

指定したフォルダ内のファイルを「指定のファイル名+連番」のファイル名として一括して指定のフォルダにコピーするアプリケーションです。  

## ライセンス

本ソフトウェアはMITライセンスにより提供されています。  
詳細は「LICENSE.txt」をご覧ください。  

## パッケージ

本ソフトウェアは以下のNuGetパッケージを使用しています。  

| パッケージ | ライセンス |
|:-----------|:-----------|
| WindowsAPICodePack-Core | [Custom](https://github.com/aybe/Windows-API-Code-Pack-1.1/blob/master/LICENCE) |
| WindowsAPICodePack-Shell | [Custom](https://github.com/aybe/Windows-API-Code-Pack-1.1/blob/master/LICENCE) |
| MahApps.Metro | [MIT](https://github.com/MahApps/MahApps.Metro/blob/develop/LICENSE) |
| ControlzEx | [MIT](https://github.com/ControlzEx/ControlzEx/blob/develop/LICENSE) |
| Microsoft.Xaml.Behaviors.Wpf |[MIT](https://github.com/microsoft/XamlBehaviorsWpf/blob/master/LICENSE) |
| System.Text.Json | [MIT](https://www.nuget.org/packages/System.Text.Json/4.7.2/license) |

## 使用方法

### Fromボタン

Fromボタンをクリックするとフォルダ選択ダイアログが表示されます。

コピーするファイルが含まれるフォルダを選択してください。

選択したコピー元フォルダが表示されると共にリストにコピーするファイルが表示されます。  
同時にコピー後のファイル名がリストに表示されます。

### Toボタン

Toボタンをクリックするとフォルダ選択ダイアログが表示されます。

ファイルをコピーする先のフォルダを選択してください。

選択したコピー先フォルダが表示されます。

コピー先フォルダを選択しない場合には、ファイルはコピー元フォルダにコピーされます。

### 共通ファイル名

ファイルをコピーする際のファイル名は「共通ファイル名+連番.拡張子」となります。

共通ファイル名に文字列を入力する事でファイル名を変更する事ができます。

### Copyボタン

Copyボタンをクリックすると実際にファイルがコピーされます。

### Closeボタン

アプリケーションを終了します。