using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SunokoLibrary.Application.Browsers
{
    /// <summary>
    /// MaxthonからICookieImporterを取得します。
    /// </summary>
    public class MaxthonBrowserManager : BrowserManagerBase
    {
#pragma warning disable 1591
        public override IEnumerable<ICookieImporter> GetCookieImporters()
        {
            var name = "Maxthon webkit";
            var path = Utility.ReplacePathSymbols(COOKIEPATH);
            if (!System.IO.File.Exists(path))
                path = null;

            var status = new BrowserConfig(name, "Default", path, EngineIds[0], false);
            return new ICookieImporter[] { new BlinkCookieGetter(status, 2) };
        }
        public override ICookieImporter GetCookieImporter(BrowserConfig config)
        { return new BlinkCookieGetter(config, 2); }
#pragma warning restore 1591

        const string COOKIEPATH = "%APPDATA%\\Maxthon3\\Users\\guest\\Cookie\\Cookie.dat";
    }
}