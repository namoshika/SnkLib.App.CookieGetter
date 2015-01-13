using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.ServiceModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SunokoLibrary.Application.Browsers
{
    /// <summary>
    /// 保護モードIEブラウザからCookieを取得します。
    /// </summary>
    public class IEPMCookieImporter : IECookieImporter
    {
#pragma warning disable 1591

        public IEPMCookieImporter(BrowserConfig config, int primaryLevel) : base(config, primaryLevel) { }

        public override bool IsAvailable { get { return Win32Api.GetIEVersion().Major >= 8; } }
        public override ICookieImporter Generate(BrowserConfig config)
        { return new IEPMCookieImporter(config, PrimaryLevel); }
        protected override ImportResult ProtectedGetCookies(Uri targetUrl)
        {
            if (IsAvailable == false)
                return new ImportResult(null, ImportState.Unavailable);
            try
            {
                var cookiesText = PrivateGetCookiesWinApi(targetUrl, null);
                Debug.Assert(cookiesText != null, "IEGetProtectedModeCookie: error");
                if (cookiesText != null)
                {
                    var cookies = new CookieCollection();
                    foreach (var item in ParseCookies(cookiesText, targetUrl))
                        cookies.Add(item);
                    return new ImportResult(cookies, ImportState.Success);
                }
                else
                    return new ImportResult(null, ImportState.AccessError);
            }
            catch (CookieImportException ex)
            {
                TraceFail(this, "Cookie読み込みに失敗。", ex.ToString());
                return new ImportResult(null, ex.Result);
            }
        }

#pragma warning restore 1591

        string PrivateGetCookiesWinApi(Uri url, string key)
        {
#if DEBUG
            //動作確認用
            //-1:分岐指定なし、0:IE11以上時の処理、1:IE8以上でx64の時の処理
            var specifyPath_Debug = -1;
#else
            var specifyPath_Debug = -1;
#endif
            var ieVersion = Win32Api.GetIEVersion();
            //IEのバージョンによって使えるAPIに違いがあるため、分岐させる。
            //IE11以上はCookie取得APIを使用する。IE11からはx64モード下でも使用可能になっている。
            //IE8以上もx86環境では問題ないので一緒に取得させておく。
            if ((ieVersion.Major >= 11 || ieVersion.Major >= 8 && Environment.Is64BitProcess == false) && specifyPath_Debug < 0 || specifyPath_Debug == 0)
            {
                string lpszCookieData;
                var hResult = Win32Api.GetCookiesFromProtectedModeIE(out lpszCookieData, url, key);
                Debug.Assert(
                    lpszCookieData != null, string.Format("win32api.GetCookieFromProtectedModeIE error code:{0}", hResult));
                return lpszCookieData ?? string.Empty;
            }
            //IE8以上はCookie取得APIを使用する。
            //x64モード下での使用は未対応なのでx86の子プロセスを経由させる
            else if (ieVersion.Major >= 8 && specifyPath_Debug < 0 || specifyPath_Debug == 1)
            {
                var processId = Process.GetCurrentProcess().Id.ToString();
                var endpointUrl = new Uri(string.Format("net.pipe://localhost/SnkLib.App.CookieGetter.x86Proxy/{0}/Service/", processId));
                var lpszCookieData = string.Empty;
                ChannelFactory<IProxyService> proxyFactory = null;
                Process proxyProcess = null;
                //多重呼び出しされる事がよくあるため、既に起動しているx86ProxyServiceの存在を期待する。
                //初回呼び出しなど期待外れもあり得るので2回は試行する。
                for (var i = 0; i < 2; i++)
                    try
                    {
                        proxyFactory = new ChannelFactory<IProxyService>(new NetNamedPipeBinding(), endpointUrl.AbsoluteUri);
                        var proxy = proxyFactory.CreateChannel();
                        var hResult = proxy.GetCookiesFromProtectedModeIE(out lpszCookieData, url, key);
                        Debug.Assert(
                            lpszCookieData != null, string.Format("proxy.GetCookieFromProtectedModeIE error code:{0}", hResult));
                        break;
                    }
                    catch (CommunicationException)
                    {
                        //x86Serviceからの起動完了通知受信用
                        using (var pipeServer = new System.IO.Pipes.AnonymousPipeServerStream(
                            System.IO.Pipes.PipeDirection.In, HandleInheritability.Inheritable))
                        {
                            proxyProcess = Process.Start(
                                new System.Diagnostics.ProcessStartInfo()
                                {
                                    FileName = ".\\SnkLib.App.CookieGetter.x86Proxy.exe",
                                    //サービス側のendpointUrlに必要な情報をコマンドライン引数として渡す
                                    Arguments = string.Join(" ", new[] { processId, pipeServer.GetClientHandleAsString(), }),
                                    CreateNoWindow = true,
                                    UseShellExecute = false,
                                });
                            pipeServer.ReadByte();
                        }
                    }
                    finally { proxyFactory.Abort(); }
                return lpszCookieData ?? string.Empty;
            }
            else
                return string.Empty;
        }
    }
}