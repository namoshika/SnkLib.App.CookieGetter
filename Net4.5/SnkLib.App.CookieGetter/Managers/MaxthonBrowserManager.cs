using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SunokoLibrary.Application.Browsers
{
    /// <summary>
    /// MaxthonからICookieImporterを取得します。
    /// </summary>
    public class MaxthonBrowserManager : ICookieImporterFactory
    {
#pragma warning disable 1591
        public IEnumerable<ICookieImporter> GetCookieImporters()
        {
            var name = "Maxthon webkit";
            var path = Utility.ReplacePathSymbols(COOKIEPATH);
            if (!System.IO.File.Exists(path))
                path = null;

            var status = new BrowserConfig(name, "Default", path, BlinkBrowserManager.ENGINE_ID, false);
            return new ICookieImporter[] { new BlinkCookieGetter(status, 2) };
        }
#pragma warning restore 1591

        const string COOKIEPATH = "%APPDATA%\\Maxthon3\\Users\\guest\\Cookie\\Cookie.dat";
    }
}