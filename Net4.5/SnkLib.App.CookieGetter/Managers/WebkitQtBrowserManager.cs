using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunokoLibrary.Application.Browsers
{
    public class WebkitQtBrowserManager : BrowserManagerBase
    {
        public WebkitQtBrowserManager() : base(new[] { ENGINE_ID }) { }
        internal const string ENGINE_ID = "WebkitQt";
        public override IEnumerable<ICookieImporter> GetCookieImporters()
        { return Enumerable.Empty<ICookieImporter>(); }
        public override ICookieImporter GetCookieImporter(BrowserConfig config)
        { return new WebkitQtCookieGetter(config, 2); }
    }
}
