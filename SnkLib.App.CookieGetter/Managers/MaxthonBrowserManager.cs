using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SunokoLibrary.Application.Browsers
{
    public class MaxthonBrowserManager : ICookieImporterFactory
    {
        const string COOKIEPATH = "%APPDATA%\\Maxthon3\\Users\\guest\\Cookie\\Cookie.dat";
        public IEnumerable<ICookieImporter> GetCookieImporters()
        {
            var name = "Maxthon webkit";
            var path = Utility.ReplacePathSymbols(COOKIEPATH);
            if (!System.IO.File.Exists(path))
                path = null;

            BrowserConfig status = new BrowserConfig(name, "Default", path);
            return new ICookieImporter[] { new BlinkCookieGetter(status) };
        }
    }
}