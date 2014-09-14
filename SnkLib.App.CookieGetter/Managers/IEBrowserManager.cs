using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SunokoLibrary.Application.Browsers
{
    /// <summary>
    /// IE系のすべてのクッキーを取得する
    /// </summary>
    public class IEBrowserManager : ICookieImporterFactory
    {
        public ICookieImporter[] CreateCookieImporters()
        {
            var cookieFolder = Environment.GetFolderPath(Environment.SpecialFolder.Cookies);
            return new[]{
                CreateIECookieGetter(),
                CreateIEPMCookieGetter(),
                CreateIEEPMCookieGetter(),
            };
        }
        public ICookieImporter CreateIECookieGetter()
        {
            var cookieFolder = Environment.GetFolderPath(Environment.SpecialFolder.Cookies);
            return new IECookieGetter(new BrowserConfig("IE Normal", "Default", cookieFolder));
        }
        public ICookieImporter CreateIEPMCookieGetter()
        {
            var cookieFolder = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Cookies), "low");
            return new IEPMCookieGetter(new BrowserConfig("IE Protected", "Default", cookieFolder));
        }
        public ICookieImporter CreateIEEPMCookieGetter()
        {
            var cookieFolder = Utility.ReplacePathSymbols(
                @"%LOCALAPPDATA%\Packages\windows_ie_ac_001\AC\INetCookies");
            return new IEFindCacheCookieGetter(
                new BrowserConfig("IE Enhanced Protected", "Default", cookieFolder));
        }
    }
}