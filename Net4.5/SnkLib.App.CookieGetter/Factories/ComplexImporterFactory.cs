using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SunokoLibrary.Application.Browsers
{
    /// <summary>
    /// 複数のFactoryを内部で保持し、それらを1つのFactoryとして扱えるようにする仕組みを提供します。
    /// </summary>
    public class ComplexImporterFactory : ICookieImporterFactory
    {
#pragma warning disable 1591

        public ComplexImporterFactory(IEnumerable<ICookieImporterFactory> innerFactories)
        {
            _pnirBrowserManagers = innerFactories.ToDictionary(factory => factory.EngineIds[0]);
            EngineIds = _pnirBrowserManagers.Keys.ToArray();
        }
        public string[] EngineIds { get; private set; }
        readonly Dictionary<string, ICookieImporterFactory> _pnirBrowserManagers;

        public IEnumerable<ICookieImporter> GetCookieImporters()
        {
            var importers = _pnirBrowserManagers
                .SelectMany(pair => pair.Value.GetCookieImporters()
                .ToArray());
            return importers;
        }
        public ICookieImporter GetCookieImporter(CookieSourceInfo sourceInfo)
        {
            ICookieImporterFactory manager;
            if (!_pnirBrowserManagers.TryGetValue(sourceInfo.EngineId, out manager))
                throw new ArgumentException("引数infoのEngineIdsに対応していません。");
            return manager.GetCookieImporter(sourceInfo);
        }

#pragma warning restore 1591
    }
}
