using System;
using System.Collections.Generic;
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
            var getters = new List<ICookieImporter>();
            getters.Add(new IECookieGetter(new BrowserConfig("InternetExplorer", "Default", cookieFolder)));

            if (System.IO.Directory.Exists(System.IO.Path.Combine(cookieFolder, "low")))
                getters.Add(new IEPMCookieGetter(new BrowserConfig("InternetExplorer Protected", "Default", cookieFolder)));
            return getters.ToArray();
        }
        public ICookieImporter CreateIECookieGetter()
        {
            var cookieFolder = Environment.GetFolderPath(Environment.SpecialFolder.Cookies);
            return new IECookieGetter(new BrowserConfig("InternetExplorer", "Default", cookieFolder));
        }
        public ICookieImporter CreateIEPMCookieGetter()
        {
            var cookieFolder = Environment.GetFolderPath(Environment.SpecialFolder.Cookies);
            return new IEPMCookieGetter(new BrowserConfig("InternetExplorer Protected", "Default", cookieFolder));
        }
    }
}