#SnkLib.App.CookieGetter

ブラウザのCookieを.NETアプリで使えるようにするライブラリです。  
<http://com.nicovideo.jp/community/co235502> で配布されているCookieGetterSharpを元に、互換性を維持した上での設計の改善を施したものです。.NET4.0以上で動きます。

本家と同水準でIE, Chrome, Firefox, Lunascape, Maxthon, Sleipnir, Tungsten などに対応しています。また、Kinzaなどの本家が対応していない派生ブラウザ系統への包括的な対応と設定保存周りの使い勝手の改善などが行われています。

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
現在、以下のブランチの方針下にあります。
masterで本家との互換性を追究しつつ、Gecko, Webkit系のIBrowserManagerのマルチプロファイル周りの重複コード除去や各種コードのリファクタリング、設定保存周りの使い勝手向上を目指した改造が行われています。

* base: 本家そのまんま。
* baseFix: 見つけた不具合の修正。互換性重視で触らぬ神に祟りなし方針。
* master: 設計の改善。

また、プロジェクトは以下の方針下にあります。  
各プロジェクトはフレームワークのバージョン毎にフォルダを分けられています。

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

##使い方(開発者向け)
使用したいプロジェクトへNuGetで以下のパッケージをインストールします。
* [SnkLib.App.CookieGetter](https://www.nuget.org/packages/SnkLib.App.CookieGetter/)を追加する(必須)。
* [SnkLib.App.CookieGetter.Forms](https://www.nuget.org/packages/SnkLib.App.CookieGetter.Forms/)を追加する  
  (オプション。Windows Forms向けのUI部品が入っています。)。

以下の解説は新クラスの使い方です。本家とは設計が異なります。本家と同じ設計を使いたい場合にはNET4.5フォルダ内のCookieGetterSharpを使用します。オススメはしません。

```C#
//以下の名前空間を参照します。
using SunokoLibrary.Application;

//対応ブラウザからのCookieGetterリストの取得をします。
var importableBrowsers = await CookieGetters.GetInstancesAsync(true);

//Cookieの取得は以下のようにします。
//引数として指定されたCookieContainerに取得結果を追加していく設計です。
var cookies = new CookieContainer();
var cookieGetter = importableBrowsers.First();
var targetUrl = new Uri("http://nicovideo.jp/");
await cookieGetter.GetCookiesAsync(targetUrl, cookies);

//個別の値を取得したい場合は以下のようにします。
var cookie = cookies.GetCookies(targetUrl)["user_session"];

//次回起動時用の構成を保存します。
Properties.Settings.Default.BrowserName = cookieGetter.Config.BrowserName;
Properties.Settings.Default.ProfileName = cookieGetter.Config.ProfileName;
Properties.Settings.Default.CookiePath = cookieGetter.Config.CookiePath;
Properties.Settings.Default.Save();
//直接的に構成を保存することもできます。
Properties.Settings.Default.SelectedBrowserConfig = cookieGetter.Config

//任意のBrowserConfigから適切なGetterを取得します。
//適切なものが見つからない場合は適当なのを見繕うなど、次回起動時の設定の
//復元が楽になるように作っています。
var currentGetter = await CookieGetters.GetInstanceAsync(
  Properties.Settings.Default.SelectedBrowserConfig);
```
