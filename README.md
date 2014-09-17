#SnkLib.App.CookieGetter

ブラウザのCookieを.NETアプリで使えるようにするライブラリです。  
<http://com.nicovideo.jp/community/co235502> で配布されているCookieGetterSharpを元に、互換性を維持した上での設計の改善を施したものです。

オリジナルは炬燵犬さん作成のクッキー取得クラス [CookieGetter](http://homepage2.nifty.com/kotatuinu/contents/computer/program/CookieGetter/cookiegetter.html)、それを大幅に改造しライブラリにしたhalxxxxさん、うつろさんの[CookieGetterSharp](http://d.hatena.ne.jp/halxxxx/20091212/1260649353)が存在します。ライセンスはCookieGetterSharpのを継承させます。

## ライセンス
コードは自由にご利用ください。ですが悪用は厳禁です。  
またこのライブラリを使用したことによっていかなる損害が発生しても責任は一切持ちません。  
あと、C#の更なる発展のため、改造したソースは公開してください。  

##方針
現在、以下のブランチの方針下にあります。
masterで本家との互換性を追究しつつ、Gecko, Webkit系のIBrowserManagerのマルチプロファイル周りの重複コード除去や各種コードのリファクタリング、設定保存周りの使い勝手向上を目指した改造が行われています。

* base: 本家そのまんま。
* baseFix: 見つけた不具合の修正。互換性重視で触らぬ神に祟りなし方針。
* master: 設計の改善。

また、プロジェクトは以下の方針下にあります。

* CookieGetterSharp: 本家と互換性を持ったせるラッパー。
* NwhoisLoginSystem: 本家に付いてるデモアプリ。
* Sample: 新クラスに対応させたデモアプリ。
* SnkLib.App.CookieGetter: 本体。
* SnkLib.App.CookieGetter.x86Proxy: 本体が内部で使用する子プロセス。
* UnitTests: 動作確認。

##使い方
* ライブラリを使用したいプロジェクトの参照にSnkLib.App.CookieGetter.dllを追加します(必須)。
* ビルド結果のSnkLib.App.CookieGetter.dllと同階層には以下のファイル、フォルダも配置します。
  * SnkLib.App.CookieGetter.x86Proxy.exe(必須)。
  * Win32, x64フォルダを中身ごと(必須)。
  * SnkLib.App.CookieGetter.dll.config(必須)。
  * SnkLib.App.CookieGetter.xml(オプション。あると幸せ)。
* CookieGetterSharp.dllを参照へ追加します(不要。本家流の時のみ必須)。

以下の解説は新クラス流の使い方です。本家流の使い方は本家のページを読んでください。

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
  new BrowserConfig(
    Properties.Settings.Default.BrowserName,
    Properties.Settings.Default.ProfileName,
    Properties.Settings.Default.CookiePath));
```