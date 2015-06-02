
#SnkLib.App.CookieGetter

ブラウザのCookieを.NETアプリで使えるようにするライブラリです。  
<http://com.nicovideo.jp/community/co235502> で配布されているCookieGetterSharpを元に、設計の改善を施しています。.NET4.0以上で動きます。

本家と同水準でIE, Chrome, Firefox, Lunascape, Maxthon, Sleipnir, Tungsten などに対応しています。加えてKinzaなどの本家が未対応の派生ブラウザへの包括的な対応と設定保存周りの使い勝手の改善が行われています。

また、これをベースとした互換ライブラリと既存のアプリのCookieGetterSharpを差し替える事でこのライブラリの強化機能を得る使い方も出来ます(.NET2.0以上)。互換ライブラリはReleasesにSnkLib.App.CookieGetter.Sharpとしてビルド済みのものが配布されています。

## ライセンス
以下のライセンス下にあります。  
* SnkLib.App.CookieGetter  
  https://github.com/namoshika/SnkLib.App.CookieGetter  
  Copyright (c) 2014 namoshika.  
  Released under the **GNU Lesser GPL**  

以下の著作物から派生させています。
* [CookieGetterSharp](http://d.hatena.ne.jp/halxxxx/20091212/1260649353)  
  Copyright (c) 2014 halxxxx, うつろ

##方針
プロジェクトは以下の方針下にあります。  
各プロジェクトはフレームワークのバージョン毎にフォルダを分けられています。

* NET2.0
  * NwhoisLoginSystem: 本家に付いてるデモアプリ。
  * CookieGetterSharp:
    本家との互換ライブラリ。本家が対応していないブラウザへの対応などを既存のアプリに提供する。
* NET4.0
  * SnkLib.App.CookieGetter: 本体。
  * SnkLib.App.CookieGetter.Forms: ブラウザ選択UIなどの部品。
  * SnkLib.App.CookieGetter.x86Proxy: 本体が内部で使用する子プロセス。.NET4.5と共用。
  * Sample: 新クラスに対応させたデモアプリ。
* NET4.5
  * SnkLib.App.CookieGetter: 本体。
  * SnkLib.App.CookieGetter.Forms: ブラウザ選択UIなどの部品。
  * Sample: 新クラスに対応させたデモアプリ。
  * NwhoisLoginSystem: 本家に付いてるデモアプリ。
  * CookieGetterSharp:
    本家との互換ライブラリ。本家が対応していないブラウザへの対応などを既存のアプリに提供する。
* Nuspecs: NuGetパッケージ生成関係。
* Publish: 生成したnupkgの出力先
* UnitTests: 動作確認。

##使い方
使用したいプロジェクトへNuGetで以下のパッケージをインストールします。
* [SnkLib.App.CookieGetter](https://www.nuget.org/packages/SnkLib.App.CookieGetter/)を追加する(必須)。
* [SnkLib.App.CookieGetter.Forms](https://www.nuget.org/packages/SnkLib.App.CookieGetter.Forms/)を追加する  
  (オプション。Windows Forms向けのUI部品が入っています。)。

```C#
//以下の名前空間を参照します。
using SunokoLibrary.Application;

//対応ブラウザからのCookieGetterリストの取得をします。
var importableBrowsers = await CookieGetters.Default.GetInstancesAsync(true);

//Cookieの取得は以下のようにします。
//引数として指定されたCookieContainerに取得結果を追加していく設計です。
var cookieGetter = importableBrowsers.First();
var targetUrl = new Uri("http://nicovideo.jp/");
var result = await cookieGetter.GetCookiesAsync(targetUrl);

//通信に使う際にはCookieContainerへ取得結果を追加して使用します。
var cookies = new CookieContainer();
cookies.Add(result.Cookies);
//個別の値を取得したい場合は以下のようにします。
var cookie = result.Cookies["user_session"];

//次回起動時用の構成を保存します。
Properties.Settings.Default.SelectedGetterInfo = cookieGetter.SourceInfo

//任意のBrowserConfigから適切なGetterを取得します。
//適切なものが見つからない場合は適当なのを見繕うなど、次回起動時の設定の
//復元が楽になるように作っています。
var currentGetter = await CookieGetters.Default.GetInstanceAsync(
  Properties.Settings.Default.SelectedGetterInfo);

//ブラウザを指定してGetterを取得することもできます。
var chromeGetter = CookieGetters.Browsers.Chrome;
```
