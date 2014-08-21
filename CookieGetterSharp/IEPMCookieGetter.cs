using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.ServiceModel;
using System.Text;
using System.Runtime.InteropServices;

namespace Hal.CookieGetterSharp {

    /// <summary>
    /// 保護モードIEブラウザのクッキーを取得する
    /// </summary>
    class IEPMCookieGetter : AIECookieGetter {

        static Random _proxyIdGenerator = new Random();
        public IEPMCookieGetter(CookieStatus status, bool checkSubDirectory)
            : base(status, checkSubDirectory) {
        }

        /// <summary>
        /// 対象URL上の名前がKeyであるクッキーを取得する
        /// </summary>
        /// <param name="url"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public override System.Net.Cookie GetCookie(Uri url, string key) {
            {
                // win32apiを使いパフォーマンスを向上
                List<System.Net.Cookie> cookieDatas = GetCookiesWinApi(url, key);
                if(cookieDatas.Count == 1) {
                    return cookieDatas.ToArray()[0];
                }
            }

            return base.GetCookie(url, key);
        }

        /// <summary>
        /// urlに関連付けられたクッキーを取得します。
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public override System.Net.CookieCollection GetCookieCollection(Uri url) {
            // win32apiを使いパフォーマンスを向上
            List<System.Net.Cookie> cookies = GetCookiesWinApi(url, null);

            if(cookies.Count == 0) {
                //関係のあるファイルだけ調べることによってパフォーマンスを向上させる
                List<string> files = SelectFiles(url, GetAllFiles());

                cookies = new List<System.Net.Cookie>();
                foreach(string filepath in files) {
                    cookies.AddRange(PickCookiesFromFile(filepath));
                }
            }

            // Expiresが最新のもで上書きする
            cookies.Sort(CompareCookieExpiresAsc);
            System.Net.CookieCollection collection = new System.Net.CookieCollection();
            foreach(System.Net.Cookie cookie in cookies) {
                try {
                    collection.Add(cookie);
                }
                catch(Exception ex) {
                    CookieGetter.Exceptions.Enqueue(ex);
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }

            return collection;
        }

        public override System.Net.CookieContainer GetAllCookies() {
            return base.GetAllCookies();
        }

        /// <summary>
        /// win32apiを使って対象URL上の名前がKeyであるクッキーを取得します
        /// </summary>
        /// <param name="url"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        protected override List<System.Net.Cookie> GetCookiesWinApi(Uri url, string key) {
            var cookies = new List<System.Net.Cookie>();
            var lpszCookieData = PrivateGetCookiesWinApi(url, key);

            if(lpszCookieData.Length != 0) {
                Debug.WriteLine(lpszCookieData);
                string[] cookieDatas = lpszCookieData.ToString().Split(';');
                foreach(var data in cookieDatas) {
                    System.Net.Cookie cookie = new System.Net.Cookie();
                    string[] chunks = data.ToString().Split('=');
                    if(2 <= chunks.Length) {
                        try {
                            cookie.Name = chunks[0].Trim();
                            cookie.Value = chunks[1].Trim();
                            cookie.Domain = AddDotDomain(url.Host);
                            cookie.Path = url.Segments[0];	// このほうがいいきがする 2011-11-19
                            // cookie.Path = url.AbsolutePath;
                            cookie.Expires = DateTime.Now.AddDays(30);	// 有効期限適当付与 2013-07-03
                            cookies.Add(cookie);
                        }
                        catch(System.Net.CookieException e) {
                            Debug.WriteLine(e.Message);
                        }
                    }
                }
            }

            return cookies;
        }
        string PrivateGetCookiesWinApi(Uri url, string key) {
#if DEBUG
            //動作確認用
            //-1:分岐指定なし、0:IE11以上時の処理、1:IE8以上でx64の時の処理
            var specifyPath_Debug = -1;
#else
            var specifyPath_Debug = -1;
#endif
            var ieVersion = win32api.GetIEVersion();
            //IEのバージョンによって使えるAPIに違いがあるため、分岐させる。
            //IE11以上はクッキー取得APIを使用する。IE11からはx64モード下でも使用可能になっている。
            //IE8以上もx86環境では問題ないので一緒に取得させておく。
            if((ieVersion.Major >= 11 || ieVersion.Major >= 8 && Environment.Is64BitProcess == false) && specifyPath_Debug < 0 || specifyPath_Debug == 0) {
                var lpszCookieData = string.Empty;
                var hResult = win32api.GetCookiesFromProtectedModeIE(out lpszCookieData, url, key);
                Debug.WriteLineIf(
                    lpszCookieData == null, string.Format("win32api.GetCookieFromProtectedModeIE error code:{0}", hResult));
                return lpszCookieData;
            }
            //IE8以上はクッキー取得APIを使用する。
            //x64モード下での使用は未対応なのでx86の子プロセスを経由させる
            else if(ieVersion.Major >= 8 && specifyPath_Debug < 0 || specifyPath_Debug == 1) {
                var threadId = _proxyIdGenerator.Next().ToString();
                var endpointUrl = new Uri(string.Format("net.pipe://localhost/CookieGetterSharp.x86Proxy/{0}/Service/", threadId));
                var lpszCookieData = string.Empty;
                ChannelFactory<IX86ProxyService> proxyFactory = null;
                using(var proxyProcess = Process.Start(
                    new System.Diagnostics.ProcessStartInfo() {
                        FileName = ".\\CookieGetterSharp.x86Proxy.exe",
                        Arguments = threadId,
                        CreateNoWindow = true,
                        UseShellExecute = false
                    }))
                    //失敗しても2度目の正直を狙う
                    for(var i = 0; i < 2; i++)
                        try {
                            proxyFactory = new ChannelFactory<IX86ProxyService>(new NetNamedPipeBinding(), endpointUrl.AbsoluteUri);
                            var proxy = proxyFactory.CreateChannel();
                            var hResult = proxy.GetCookiesFromProtectedModeIE(out lpszCookieData, url, key);
                            Debug.WriteLineIf(
                                lpszCookieData == null, string.Format("proxy.GetCookieFromProtectedModeIE error code:{0}", hResult));
                            break;
                        }
                        catch(EndpointNotFoundException) {
                            //x86Serviceが起動しきっていない場合は少し待ってから再試行する。
                            //苦しい作りだが指定時間スリープで完全に起動し切るのを待つ
                            System.Threading.Thread.Sleep(300);
                        }
                        catch(CommunicationException) {
                            //x86Serviceが起動しきっていない場合は少し待ってから再試行する。
                            //苦しい作りだが指定時間スリープで完全に起動し切るのを待つ
                            System.Threading.Thread.Sleep(300);
                        }
                        finally {
                            proxyFactory.Abort();
                            proxyProcess.CloseMainWindow();
                        }
                return lpszCookieData;
            }
            else
                return string.Empty;
        }
    }
}
