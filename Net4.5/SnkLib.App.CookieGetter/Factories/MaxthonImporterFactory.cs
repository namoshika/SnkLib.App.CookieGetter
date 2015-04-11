using System;
using System.Collections.Generic;
using System.Text;

namespace SunokoLibrary.Application.Browsers
{
    /// <summary>
    /// MaxthonからICookieImporterを取得します。
    /// </summary>
    public class MaxthonImporterFactory : ImporterFactoryBase
    {
#pragma warning disable 1591
        public override IEnumerable<ICookieImporter> GetCookieImporters()
        {
            var name = "Maxthon Webkit";
            var path = Utility.ReplacePathSymbols(COOKIEPATH);
            if (!System.IO.File.Exists(path))
                path = null;

            var info = new CookieSourceInfo(name, "Default", path, EngineIds[0], false);
            return new ICookieImporter[] { new BlinkCookieImporter(info, 2) };
        }
        public override ICookieImporter GetCookieImporter(CookieSourceInfo sourceInfo)
        { return new BlinkCookieImporter(sourceInfo, 2); }
#pragma warning restore 1591

        const string COOKIEPATH = "%APPDATA%\\Maxthon3\\Users\\guest\\Cookie\\Cookie.dat";
    }
}