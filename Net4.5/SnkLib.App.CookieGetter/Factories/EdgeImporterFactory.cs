using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SunokoLibrary.Application.Browsers
{
    /// <summary>
    /// SpartanのICookieImporterを取得します。
    /// </summary>
    public class EdgeImporterFactory : ImporterFactoryBase
    {
#pragma warning disable 1591
        public EdgeImporterFactory() : base() { }
        public override IEnumerable<ICookieImporter> GetCookieImporters()
        {
            if (_importer == null)
            {
                var cookieFolder = Utility.ReplacePathSymbols(@"%LOCALAPPDATA%\Packages\");
                IEnumerable<string> seq;
                try { seq = System.IO.Directory.GetDirectories(cookieFolder, "Microsoft.MicrosoftEdge_*"); }
                catch (System.IO.DirectoryNotFoundException) { seq = Enumerable.Empty<string>(); }

                cookieFolder = seq
                    .DefaultIfEmpty(cookieFolder + @"Microsoft.MicrosoftEdge_xxx")
                    .FirstOrDefault();
                cookieFolder += @"\AC\#!001\MicrosoftEdge\Cookies";
                _importer = new IEFindCacheCookieImporter(
                    new CookieSourceInfo("MicrosoftEdge", "Default", cookieFolder, EngineIds[0], false), 0);
            }
            return new[] { _importer };
        }
        public override ICookieImporter GetCookieImporter(CookieSourceInfo sourceInfo)
        { return new IEFindCacheCookieImporter(sourceInfo, 0); }
#pragma warning restore 1591

        static ICookieImporter _importer;
    }
}
