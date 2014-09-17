using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace SunokoLibrary.Application.Browsers
{
    public class LunascapeWebkitBrowserManager : ICookieImporterFactory
    {
        const string LUNASCAPE_PLUGIN_FOLDER = "%APPDATA%\\Lunascape\\Lunascape6\\plugins";
        const string COOKIEPATH = "data\\cookies.ini";

        public IEnumerable<ICookieImporter> CreateCookieImporters()
        {
            var path = SearchCookieDirectory();
            var option = new BrowserConfig("Lunascape Webkit", "Default", path);
            return new ICookieImporter[] { new WebkitQtCookieGetter(option) };
        }
        /// <summary>
        /// Lunascape6のプラグインフォルダからFirefoxのクッキーが保存されているパスを検索する
        /// </summary>
        /// <returns></returns>
        string SearchCookieDirectory()
        {
            var pluginDir = Utility.ReplacePathSymbols(LUNASCAPE_PLUGIN_FOLDER);
            string cookiePath = null;
            if (System.IO.Directory.Exists(pluginDir))
                cookiePath = Directory.EnumerateDirectories(pluginDir)
                    .Select(child => Path.Combine(child, COOKIEPATH))
                    .Where(child => File.Exists(child))
                    .FirstOrDefault();
            return cookiePath;
        }
    }
}