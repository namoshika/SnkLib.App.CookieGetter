using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace SunokoLibrary.Application.Browsers
{
    /// <summary>
    /// LunascapeからICookieImporterを取得します。
    /// </summary>
    public class LunascapeBrowserManager : ComplexBrowserManager
    {
#pragma warning disable 1591
        public LunascapeBrowserManager()
            : base(new ICookieImporterFactory[] {
                new LunascapeGeckoBrowserManager(),
                new LunascapeWebkitBrowserManager(),
            }) { }
#pragma warning restore 1591

        class LunascapeGeckoBrowserManager : BrowserManagerBase
        {
            const string LUNASCAPE_PLUGIN_FOLDER5 = "%APPDATA%\\Lunascape\\Lunascape5\\ApplicationData\\gecko\\cookies.sqlite";
            const string LUNASCAPE_PLUGIN_FOLDER6 = "%APPDATA%\\Lunascape\\Lunascape6\\plugins";
            const string COOKIEPATH = "data\\cookies.sqlite";

            public override IEnumerable<ICookieImporter> GetCookieImporters()
            {
                var path = SearchCookieDirectory();
                var status = new BrowserConfig("Lunascape Gecko", "Default", path, EngineIds[0], false);
                return new ICookieImporter[] { new GeckoCookieGetter(status, 2) };
            }
            public override ICookieImporter GetCookieImporter(BrowserConfig config)
            { return new GeckoCookieGetter(config, 2); }
            /// <summary>
            /// Lunascape6のプラグインフォルダからFirefoxのCookieが保存されているパスを検索します。
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
        class LunascapeWebkitBrowserManager : BrowserManagerBase
        {
            const string LUNASCAPE_PLUGIN_FOLDER = "%APPDATA%\\Lunascape\\Lunascape6\\plugins";
            const string COOKIEPATH = "data\\cookies.ini";

            public override IEnumerable<ICookieImporter> GetCookieImporters()
            {
                var path = SearchCookieDirectory();
                var option = new BrowserConfig("Lunascape Webkit", "Default", path, EngineIds[0], false);
                return new ICookieImporter[] { new WebkitQtCookieGetter(option, 2) };
            }
            public override ICookieImporter GetCookieImporter(BrowserConfig config)
            { return new WebkitQtCookieGetter(config, 2); }
            /// <summary>
            /// Lunascape6のプラグインフォルダからFirefoxのCookieが保存されているパスを検索する
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