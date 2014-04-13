#CookieGetterSharp

<http://com.nicovideo.jp/community/co235502> で配布されているCookieGetterSharpが便利そうだったので拝借。元としたソースファイルのバージョンは "**CookieGetterSharp20140223**" です。追従できる範囲内でmasterブランチに本家の変更を反映し、私が加えた変更を別のブランチで行う方針です。

オリジナルは炬燵犬さん作成のクッキー取得クラス [CookieGetter](http://homepage2.nifty.com/kotatuinu/contents/computer/program/CookieGetter/cookiegetter.html)、それを大幅に改造しライブラリにしたhalxxxxさん、うつろさんの[CookieGetterSharp](http://d.hatena.ne.jp/halxxxx/20091212/1260649353)、そしてさらにそれを改造したのがこの配布物です。ライセンスはCookieGetterSharpのを継承させます。

---
## ライセンス
コードは自由にご利用ください。ですが悪用は厳禁です。  
またこのライブラリを使用したことによっていかなる損害が発生しても責任は一切持ちません。  
あと、C#の更なる発展のため、改造したソースは公開してください。  

---
##使い方

###各クラスの役割
このライブラリは主に以下のクラスが存在します。利用者が使用するのは主にCookieGetter, ICookieGetterのみとなります。CookieStatusはICookieGetterに保持され、Cookie読み込みの可不可などを保持しますが、ICookieGetter実装クラスのコードを読む際はCookieStatusをCookie取得のパラメータ類と見なしたほうが読みやすいです。

* static CookieGetter
* instance IBrowserManager
* instance ICookieGetter
* instance CookieStatus

###使用例
* なるべく独自にブラウザリストを生成せず、下のようにCreateInstancesメソッドから動的にリストを生成してください。
  > var importableBrowsers = CookieGetter.CreateInstances(true);  
  > comboBox1.Items.AddRange(importableBrowsers);  

* Cookieの取得は以下のようにします
  > importableBrowsers[0].GetCookie(new Uri("http://live.nicovideo.jp/"), "user_session") か、または、  
  > importableBrowsers[0].GetCookieCollection(new Uri("http://live.nicovideo.jp/"))  

  * 返り値を接続の都度、CookieContainerにセットしてください。 
* Hal.CookieGetterSharp.BrowserTypeを設定の保存復元に使わないでください。
