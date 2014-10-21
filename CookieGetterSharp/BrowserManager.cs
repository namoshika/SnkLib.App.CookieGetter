using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hal.CookieGetterSharp
{
    using SunokoLibrary.Application;
    using SunokoLibrary.Application.Browsers;

    class BrowserManager : IBrowserManager
    {
        public BrowserManager(BrowserType browserType, ICookieImporterFactory factory)
        {
            BrowserType = browserType;
            _factory = factory;
        }
        ICookieImporterFactory _factory;
        public BrowserType BrowserType { get; private set; }
        public ICookieGetter CreateDefaultCookieGetter()
        { return CreateCookieGetters().FirstOrDefault(); }
        public ICookieGetter[] CreateCookieGetters()
        {
            try
            {
#pragma warning disable 0618 //Obsolete属性の警告を無効化"
                return _factory.GetCookieImporters()
                    .Select(importer => new CookieGetter(importer, this))
                    .ToArray();
#pragma warning restore 0618
            }
            catch(CookieImportException e)
            { throw new CookieGetterException(e); }
        }
    }
}
