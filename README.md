# CopyFilesWithSpecifiedName #

指定したフォルダ内のファイルを「指定のファイル名+連番」のファイル名として一括して指定のフォルダにコピーするアプリケーションです。  

## バージョン ##

2.1.0

## ライセンス ##

本ソフトウェアはMITライセンスにより提供されています。  
詳細は「LICENSE.txt」をご覧ください。  

## パッケージ ##

本ソフトウェアは以下のNuGetパッケージを使用しています。  

| パッケージ                   | ライセンス                                                                                     |
|:-----------------------------|:-----------------------------------------------------------------------------------------------|
| WindowsAPICodePack-Core      | [Custom](https://github.com/aybe/Windows-API-Code-Pack-1.1/blob/master/LICENCE)                |
| WindowsAPICodePack-Shell     | [Custom](https://github.com/aybe/Windows-API-Code-Pack-1.1/blob/master/LICENCE)                |
| MahApps.Metro                | [MIT](https://github.com/MahApps/MahApps.Metro/blob/develop/LICENSE)                           |
| MahApps.Metro.IconPacks      | [MIT](https://github.com/MahApps/MahApps.Metro.IconPacks/blob/develop/LICENSE)                 |
| ControlzEx                   | [MIT](https://github.com/ControlzEx/ControlzEx/blob/develop/LICENSE)                           |
| Microsoft.Xaml.Behaviors.Wpf | [MIT](https://github.com/microsoft/XamlBehaviorsWpf/blob/master/LICENSE)                       |
| System.Text.Json             | [MIT](https://www.nuget.org/packages/System.Text.Json/4.7.2/license)                           |
| MaterialDesignThemes         | [MIT](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit/blob/master/LICENSE) |
| MaterialDesignColors         | [MIT](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit/blob/master/LICENSE) |
| MaterialDesignThemes.MahApps | [MIT](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit/blob/master/LICENSE) |

## 使用方法 ##

### Toボタン ###

Toボタンをクリックするとフォルダ選択ダイアログが表示されます。

ファイルをコピーする先のフォルダを選択してください。

選択したコピー先フォルダが表示されます。

デフォルトのコピー先フォルダはドキュメントフォルダです。

### 共通ファイル名 ###

ファイルをコピーする際のファイル名は「共通ファイル名+連番.拡張子」となります。

共通ファイル名に文字列を入力してファイル名を変更できます。

なおファイル名に'?'を使用することで連番を挿入する部位を指定できます。  
'?'を使用しない場合には指定したファイル名の後に連番が挿入されます。

### 開始番号 ###

連番の開始番号を指定します。  
0以上の整数を指定できます。

### 桁数 ###

連番の最小の桁数を指定します。

連番が最小の桁数に満たない場合には頭に0を付加して最小の桁数になるように調整します。

### Addボタン ###

Addボタンをクリックするとファイル選択ダイアログが表示されます。

コピーするファイルを選択してください。

選択したコピー元ファイルがリストに表示されます。  
同時にコピー後のファイル名がリストに表示されます。

### Copyボタン ###

Copyボタンをクリックすると実際にファイルがコピーされます。

コピーするファイルのサイズが大きい場合には時間が掛かります。

ファイルコピー中はプログレスバーが表示されますので、ファイルコピーを中断したい場合にはCancelボタンをクリックしてください。

### Closeボタン ###

アプリケーションを終了します。

### 拡張子 ###

拡張子欄に拡張子を記入することでコピー対象のファイルをフィルタリングできます。

拡張子は区切り文字カンマ',', スペース' ', ピリオド'.', セミコロン';', コロン':'を使用することで複数指定できます。

拡張子の指定がない場合には、隠しファイルやシステムファイルを除く全ての拡張子のファイルがコピーの対象となります。

Addボタンでコピー元ファイルを選択する場合やドラッグアンドドロップでコピー元リストにファイルを追加する場合、拡張子欄に記入した拡張子のファイルのみ追加されます。

### Filterボタン ###

Filterボタンをクリックするとコピー元リスト中のファイルから拡張子欄に記入した拡張子以外のファイルを削除します。

### 隠しファイル / システムファイル除外 ###

チェックすることで隠しファイルやシステムファイルをコピー対象のファイルから除外します。

Addボタンでコピー元フォルダを選択する前に指定を完了している必要があります。

### Deleteボタン ###

リスト上のファイル名を選択後、Deleteボタンをクリックすることで選択したファイルをリストから除外できます。

いったん除外したファイル名を元に戻すには再度Addボタンでコピー元ファイルを選択するか、ドラッグアンドドロップでファイルをコピー元リストに追加してください。

### Upボタン / Downボタン ###

リスト上のファイル名を選択後、UpボタンとDownボタンでコピーするファイルのリスト上の順番を変えることができます。

リスト上の順番を変えることでコピー先ファイル名の連番の番号を変更できます。

### Showボタン ###

FFmpegがインストールされている場合に表示されます。

コピー元リストを選択した後にShowボタンをクリックすると、選択したファイルをFFplayを使って再生/表示します。  
ただしFFmpegが対応していないファイルについては何も起こりません。

FFmpegがインストールされているにも係わらずShowボタンが表示されない場合には、環境変数のPATHにFFmpegが設定されているか確認してください。

## 変更履歴 ##

### 2.1.0 ###

FFmpegをインストールしている場合にコピー元リストのファイルを再生もしくは表示するようにボタンを追加。

### 2.0.0 ###

- コピー元ファイル指定をフォルダ単位からファイル単位に変更。
- 連番について、開始番号の指定と桁数の指定の機能を追加。
- コピー元ファイルリストのクリアボタンを追加。
- 拡張子指定によるコピー元ファイルリストのフィルタリングボタンを追加。
- コピー元ファイルのドラッグアンドドロップ機能を追加。

### 1.0.0 ###

最初のリリース。
