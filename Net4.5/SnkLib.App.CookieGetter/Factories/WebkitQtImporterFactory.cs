using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SunokoLibrary.Application.Browsers
{
    /// <summary>
    /// WebkitQtからICookieImporterを取得します。
    /// </summary>
    public class WebkitQtImporterFactory : ImporterFactoryBase
    {
#pragma warning disable 1591
        public override IEnumerable<ICookieImporter> GetCookieImporters()
        { return Enumerable.Empty<ICookieImporter>(); }
        public override ICookieImporter GetCookieImporter(CookieSourceInfo sourceInfo)
        { return new WebkitQtCookieImporter(sourceInfo, 2); }
#pragma warning restore 1591
    }
}
