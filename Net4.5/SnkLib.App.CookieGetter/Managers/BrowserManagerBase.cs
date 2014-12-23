using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunokoLibrary.Application.Browsers
{
    /// <summary>
    /// ICookieImporterFactoryの実装の作成を支援する基盤クラスです。
    /// </summary>
    public abstract class BrowserManagerBase : ICookieImporterFactory, ICookieImporterGenerator
    {
#pragma warning disable 1591

        public BrowserManagerBase(string[] engineIds) { EngineIds = engineIds; }
        public string[] EngineIds { get; private set; }

        public abstract IEnumerable<ICookieImporter> GetCookieImporters();
        public abstract ICookieImporter GetCookieImporter(BrowserConfig config);

#pragma warning restore 1591
    }
}
