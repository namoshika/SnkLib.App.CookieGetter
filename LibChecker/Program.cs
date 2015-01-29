using SunokoLibrary.Application;
using SunokoLibrary.Application.Browsers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ApiChecker
{
    class Program
    {
        static void Main(string[] args)
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

                //Win32Api.IEGetProtectedModeCookie
                {
                    string lpszCookieData;
                    var hResult = Win32Api.GetCookiesFromProtectedModeIE(out lpszCookieData, targetUrl, valueKey);
                    Trace.WriteLine(lpszCookieData != null
                        ? string.Format("Win32Api.GetCookieFromProtectedModeIE success:{0}", hResult)
                        : string.Format("Win32Api.GetCookieFromProtectedModeIE error:{0}", hResult));
                }
                //Win32Api.IEGetProtectedModeCookie (No Flags)
                {
                    string lpszCookieData;
                    var hResult = Win32Api.GetCookiesFromProtectedModeIE(out lpszCookieData, targetUrl, valueKey, 0);
                    Trace.WriteLine(lpszCookieData != null
                        ? string.Format("Win32Api.GetCookieFromProtectedModeIE(no flags) success:{0}", hResult)
                        : string.Format("Win32Api.GetCookieFromProtectedModeIE(no flags) error:{0}", hResult));
                }
                //IEPMCookieImporter.InternalGetCookiesWinApiOnProxy
                {
                    var ieFactory = new IEImporterFactory();
                    var ieImporter = ieFactory.GetIEPMCookieImporter();
                    var cookieHeader = IEPMCookieImporter.InternalGetCookiesWinApiOnProxy(targetUrl, valueKey);
                    Trace.WriteLine(cookieHeader != null
                        ? string.Format("IEPMCookieImporter.InternalGetCookiesWinApiOnProxy success")
                        : string.Format("IEPMCookieImporter.InternalGetCookiesWinApiOnProxy error"));
                }

                Console.WriteLine();
                Console.WriteLine("ライブラリの動作チェックが完了しました。何かキーを押すとアプリが終了します。");
                Console.ReadKey();
            }
        }
    }
}
