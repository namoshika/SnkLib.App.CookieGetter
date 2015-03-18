using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SunokoLibrary.Application.Browsers
{
    /// <summary>
    /// ICookieImporterFactoryの実装の作成を支援する基盤クラスです。
    /// </summary>
    public abstract class ImporterFactoryBase : ICookieImporterFactory
    {
#pragma warning disable 1591
        public ImporterFactoryBase(IEnumerable<string> engineIds = null)
        {
            EngineIds = (engineIds ?? Enumerable.Empty<string>())
                .DefaultIfEmpty(GetType().FullName).ToArray();
        }
        public string[] EngineIds { get; protected set; }

        public abstract IEnumerable<ICookieImporter> GetCookieImporters();
        public abstract ICookieImporter GetCookieImporter(CookieSourceInfo sourceInfo);
#pragma warning restore 1591
    }
}
