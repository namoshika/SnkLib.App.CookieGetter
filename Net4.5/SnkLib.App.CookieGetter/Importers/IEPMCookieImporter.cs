using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
#if !NET20
using System.ServiceModel;
#endif

namespace SunokoLibrary.Application.Browsers
{
    /// <summary>
    /// 保護モードIEブラウザからCookieを取得します。
    /// </summary>
    public class IEPMCookieImporter : IECookieImporter
    {
#pragma warning disable 1591
        public IEPMCookieImporter(CookieSourceInfo info, int primaryLevel) : base(info, primaryLevel) { }

        public override bool IsAvailable
        {
            get
            {
                var res = Win32Api.GetIEVersion();
                return res != null ? res.Major >= 8 : false;
            }
        }
        public override ICookieImporter Generate(CookieSourceInfo newInfo)
        { return new IEPMCookieImporter(newInfo, PrimaryLevel); }
        protected override CookieImportResult ProtectedGetCookies(Uri targetUrl)
        {
            if (IsAvailable == false)
                return new CookieImportResult(null, CookieImportState.Unavailable);
            try
            {
                //IEのバージョンによって使えるAPIに違いがあるため、分岐させる。
                //APIは呼び出す環境で違いがある。IE8-x-IE10 on x64の時にはx86の子プロセスで取得させる。
                // [環境ごとの違い一覧]
                // * x<IE8 on x86,x64       使用不可
                // * IE8<=x<IE11 on x86     使用可能
                // * IE8<=x<IE11 on x64     使用不可
                // * IE11<=x on x86,x64     使用可能
                string cookiesText;
                var ieVersion = Win32Api.GetIEVersion();
                if(ieVersion == null)
                    return new CookieImportResult(null, CookieImportState.AccessError);
                if (ieVersion.Major >= 11 || ieVersion.Major >= 8 && Environment.Is64BitProcess == false)
                    cookiesText = InternalGetCookiesWinApi(targetUrl, null);
                else if (ieVersion.Major >= 8 && Environment.OSVersion.Platform == PlatformID.Win32NT)
                    cookiesText = InternalGetCookiesWinApiOnProxy(targetUrl, null);
                else
                    cookiesText = null;

                if (cookiesText != null)
                {
                    var cookies = new CookieCollection();
                    foreach (var item in ParseCookies(cookiesText, targetUrl))
                        cookies.Add(item);
                    return new CookieImportResult(cookies, CookieImportState.Success);
                }
                else
                    return new CookieImportResult(null, CookieImportState.AccessError);
            }
            catch (CookieImportException ex)
            {
                TraceError(this, "Cookie読み込みに失敗。", ex.ToString());
                return new CookieImportResult(null, ex.Result);
            }
        }
#pragma warning restore 1591

        internal static string InternalGetCookiesWinApi(Uri url, string key)
        {
            string lpszCookieData;
            var hResult = Win32Api.GetCookiesFromProtectedModeIE(out lpszCookieData, url, key);
            return lpszCookieData;
        }
        internal static string InternalGetCookiesWinApiOnProxy(Uri url, string key)
        {
            var processId = Process.GetCurrentProcess().Id.ToString();
            var endpointUrl = new Uri(string.Format("net.pipe://localhost/SnkLib.App.CookieGetter.x86Proxy/{0}/Service/", processId));
            string lpszCookieData = null;
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
#if IS_CGS
                                FileName = @".\Win32\SnkLib.App.CookieGetter.x86Proxy.exe",
#else
                                FileName = @".\x86\SnkLib.App.CookieGetter.x86Proxy.exe",
#endif
                                //サービス側のendpointUrlに必要な情報をコマンドライン引数として渡す
                                Arguments = string.Join(" ", new[] { processId, pipeServer.GetClientHandleAsString(), }),
                                CreateNoWindow = true,
                                UseShellExecute = false,
                            });
                        pipeServer.ReadByte();
                    }
                }
                finally { proxyFactory.Abort(); }
            return lpszCookieData;
        }
    }
}