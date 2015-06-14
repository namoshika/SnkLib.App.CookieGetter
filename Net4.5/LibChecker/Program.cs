using SunokoLibrary.Application;
using SunokoLibrary.Application.Browsers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibChecker
{
    class Program
    {
        static void Main(string[] args)
        {
            Proc().Wait();
            Console.WriteLine();
            Console.WriteLine("ライブラリの動作チェックが完了しました。何かキーを押すとアプリが終了します。");
            Console.ReadKey();
        }
        static async Task Proc()
        {
            using (var logStrm = new System.IO.StreamWriter(@".\trace.log", false) { AutoFlush = true })
            using (var loggerA = new System.Diagnostics.DelimitedListTraceListener(logStrm))
            using (var loggerB = new System.Diagnostics.ConsoleTraceListener())
            {
                System.Diagnostics.Trace.Listeners.Add(loggerA);
                System.Diagnostics.Trace.Listeners.Add(loggerB);

                Trace.WriteLine("------------------------------");
                Trace.WriteLine(" Environment");
                Trace.WriteLine("------------------------------");
                Trace.WriteLine(string.Format("Environment.OSVersion: {0}", Environment.OSVersion));
                Trace.WriteLine(string.Format("Environment.Is64BitOperatingSystem: {0}", Environment.Is64BitOperatingSystem));
                Trace.WriteLine(string.Format("Environment.Is64BitProcess: {0}", Environment.Is64BitProcess));
                Console.WriteLine();

                Trace.WriteLine("------------------------------");
                Trace.WriteLine(" Tests: IE");
                Trace.WriteLine("------------------------------");

                var targetUrl = new Uri("http://nicovideo.jp");
                var valueKey = null as string;

                try
                {
                    //Win32Api.IEGetProtectedModeCookie
                    string lpszCookieData;
                    var hResult = Win32Api.GetCookiesFromProtectedModeIE(out lpszCookieData, targetUrl, valueKey);
                    Trace.WriteLine(lpszCookieData != null
                        ? string.Format("Win32Api.GetCookieFromProtectedModeIE success: 0x{0}", hResult.ToString("x8"))
                        : string.Format("Win32Api.GetCookieFromProtectedModeIE error: 0x{0}", hResult.ToString("x8")));
                }
                catch (Exception e) { Trace.WriteLine(e); }
                finally { Trace.WriteLine(string.Empty); }
                try
                {
                    //Win32Api.IEGetProtectedModeCookie (No Flags)
                    string lpszCookieData;
                    var hResult = Win32Api.GetCookiesFromProtectedModeIE(out lpszCookieData, targetUrl, valueKey, 0);
                    Trace.WriteLine(lpszCookieData != null
                        ? string.Format("Win32Api.GetCookieFromProtectedModeIE(no flags) success: 0x{0}", hResult.ToString("x8"))
                        : string.Format("Win32Api.GetCookieFromProtectedModeIE(no flags) error: 0x{0}", hResult.ToString("x8")));
                }
                catch (Exception e) { Trace.WriteLine(e); }
                finally { Trace.WriteLine(string.Empty); }
                try
                {
                    //IEPMCookieImporter.InternalGetCookiesWinApiOnProxy
                    if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                    {
                        var cookieHeader = IEPMCookieImporter.InternalGetCookiesWinApiOnProxy(targetUrl, valueKey);
                        Trace.WriteLine(cookieHeader != null
                            ? string.Format("IEPMCookieImporter.InternalGetCookiesWinApiOnProxy success")
                            : string.Format("IEPMCookieImporter.InternalGetCookiesWinApiOnProxy error"));
                    }
                }
                catch (Exception e) { Trace.WriteLine(e); }
                finally { Trace.WriteLine(string.Empty); }
                try
                {
                    //IEEPMCookieImporter
                    if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                    {
                        var importer = new IEImporterFactory().GetIEEPMCookieImporter();
                        var res = await importer.GetCookiesAsync(targetUrl);
                        Trace.WriteLine(res.Status == CookieImportState.Success && res.Cookies.Count > 0
                            ? string.Format("IEEPMCookieImporter success")
                            : string.Format("IEEPMCookieImporter error"));
                    }
                }
                catch (Exception e) { Trace.WriteLine(e); }
                finally { Trace.WriteLine(string.Empty); }
                Console.WriteLine();

                Trace.WriteLine("------------------------------");
                Trace.WriteLine(" Tests: CookieGetters");
                Trace.WriteLine("------------------------------");
                try
                {
                    var getters = await CookieGetters.Default.GetInstancesAsync(false);
                    Trace.WriteLine(string.Format("Browser.Length: {0}", getters.Length));
                    for (var i = 0; i < getters.Length; i++)
                        try
                        {
                            Trace.WriteLine(string.Format("{0:00}: {1}", i, getters[i].SourceInfo.BrowserName));
                            Trace.Indent();
                            var res = await getters[i].GetCookiesAsync(targetUrl);
                            Trace.WriteLine(string.Format("Status: {0}", res.Status));
                            Trace.WriteLine(string.Format("Cookies.Count: {0}",
                                res.Status == CookieImportState.Success ? res.Cookies.Count.ToString() : "None"));
                            Trace.Unindent();
                        }
                        catch (Exception e) { Trace.WriteLine(e); }
                }
                catch (Exception e) { Trace.WriteLine(e); }
            }
        }
    }
}
