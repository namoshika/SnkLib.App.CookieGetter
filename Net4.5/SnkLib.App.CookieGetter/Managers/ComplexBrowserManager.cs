using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunokoLibrary.Application.Browsers
{
    /// <summary>
    /// 複数のFactoryを内部で保持し、それらを1つのFactoryとして扱えるようにする仕組みを提供します。
    /// </summary>
    public class ComplexBrowserManager : ICookieImporterFactory
    {
#pragma warning disable 1591

        public ComplexBrowserManager(IEnumerable<ICookieImporterFactory> innerFactories)
        {
            _pnirBrowserManagers = innerFactories.ToArray();
        }
        readonly ICookieImporterFactory[] _pnirBrowserManagers;

        public IEnumerable<ICookieImporter> GetCookieImporters()
        {
            var importers = _pnirBrowserManagers
                .SelectMany(pair => pair.GetCookieImporters()
                .ToArray());
            return importers;
        }

#pragma warning restore 1591
    }
}
