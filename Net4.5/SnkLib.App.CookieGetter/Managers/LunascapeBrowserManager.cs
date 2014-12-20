using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace SunokoLibrary.Application.Browsers
{
    public class LunascapeBrowserManager : ICookieImporterFactory
    {
        public IEnumerable<ICookieImporter> GetCookieImporters()
        {
            var importers =
                new[]{
                    _lunaGeckoBrowserManager.GetCookieImporters(),
                    _lunaWebkitBrowserManager.GetCookieImporters(),
                };
            return importers.SelectMany(item => item);
        }
        static readonly LunascapeGeckoBrowserManager _lunaGeckoBrowserManager = new LunascapeGeckoBrowserManager();
        static readonly LunascapeWebkitBrowserManager _lunaWebkitBrowserManager = new LunascapeWebkitBrowserManager();

        class LunascapeGeckoBrowserManager : ICookieImporterFactory
        {
            const string LUNASCAPE_PLUGIN_FOLDER5 = "%APPDATA%\\Lunascape\\Lunascape5\\ApplicationData\\gecko\\cookies.sqlite";
            const string LUNASCAPE_PLUGIN_FOLDER6 = "%APPDATA%\\Lunascape\\Lunascape6\\plugins";
            const string COOKIEPATH = "data\\cookies.sqlite";

            public IEnumerable<ICookieImporter> GetCookieImporters()
            {
                var path = SearchCookieDirectory();
                var status = new BrowserConfig("Lunascape Gecko", "Default", path, GeckoBrowserManager.ENGINE_ID, false);
                return new ICookieImporter[] { new GeckoCookieGetter(status, 2) };
            }
            /// <summary>
            /// Lunascape6のプラグインフォルダからFirefoxのクッキーが保存されているパスを検索する
            /// </summary>
            /// <returns></returns>
            string SearchCookieDirectory()
            {
                var cookiePath = Utility.ReplacePathSymbols(LUNASCAPE_PLUGIN_FOLDER5);
                if (System.IO.File.Exists(cookiePath))
                    return cookiePath;

                var pluginDir = Utility.ReplacePathSymbols(LUNASCAPE_PLUGIN_FOLDER6);
                cookiePath = null;
                if (Directory.Exists(pluginDir))
                    cookiePath = Directory.EnumerateDirectories(pluginDir)
                        .Select(child => Path.Combine(child, COOKIEPATH))
                        .Where(child => File.Exists(child))
                        .FirstOrDefault();
                return cookiePath;
            }
        }
        class LunascapeWebkitBrowserManager : ICookieImporterFactory
        {
            const string LUNASCAPE_PLUGIN_FOLDER = "%APPDATA%\\Lunascape\\Lunascape6\\plugins";
            const string COOKIEPATH = "data\\cookies.ini";

            public IEnumerable<ICookieImporter> GetCookieImporters()
            {
                var path = SearchCookieDirectory();
                var option = new BrowserConfig("Lunascape Webkit", "Default", path, WebkitQtBrowserManager.ENGINE_ID, false);
                return new ICookieImporter[] { new WebkitQtCookieGetter(option, 2) };
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
}