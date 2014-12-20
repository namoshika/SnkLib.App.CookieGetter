using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunokoLibrary.Application.Browsers
{
    public abstract class BrowserManagerBase : ICookieImporterFactory, ICookieImporterGenerator
    {
        public BrowserManagerBase(string[] engineIds) { EngineIds = engineIds; }
        public string[] EngineIds { get; private set; }

        public abstract IEnumerable<ICookieImporter> GetCookieImporters();
        public abstract ICookieImporter GetCookieImporter(BrowserConfig config);
    }
}
