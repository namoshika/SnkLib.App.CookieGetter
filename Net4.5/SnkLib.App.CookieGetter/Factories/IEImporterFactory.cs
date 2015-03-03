using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SunokoLibrary.Application.Browsers
{
    /// <summary>
    /// IE系のすべてのICookieImporterを取得します。
    /// </summary>
    public class IEImporterFactory : ImporterFactoryBase
    {
#pragma warning disable 1591
        public IEImporterFactory()
        { EngineIds = new[] { ENGINE_ID_NORMAL_IE, ENGINE_ID_PROTECTED_IE, ENGINE_ID_ENHANCED_PROTECTED_IE }; }
        public override IEnumerable<ICookieImporter> GetCookieImporters()
        {
            var cookieFolder = Environment.GetFolderPath(Environment.SpecialFolder.Cookies);
            return new[]{
                GetIECookieImporter(),
                GetIEPMCookieImporter(),
                GetIEEPMCookieImporter(),
            };
        }
        public override ICookieImporter GetCookieImporter(BrowserConfig config)
        {
            switch(config.EngineId)
            {
                case ENGINE_ID_NORMAL_IE:
                    return new IECookieImporter(config, 2);
                case ENGINE_ID_PROTECTED_IE:
                    return new IEPMCookieImporter(config, 2);
                case ENGINE_ID_ENHANCED_PROTECTED_IE:
                    return new IEFindCacheCookieImporter(config, 2);
                default:
                    throw new ArgumentException("引数configのEngineIdを使えるImporterが見つかりませんでした。");
            }
        }
#pragma warning restore 1591

        /// <summary>
        /// 非保護モードのIEからCookieを取得するICookieImporterを取得します。
        /// </summary>
        public ICookieImporter GetIECookieImporter()
        {
            var cookieFolder = Environment.GetFolderPath(Environment.SpecialFolder.Cookies);
            return new IECookieImporter(new BrowserConfig(
                "IE Normal", "Default", cookieFolder, ENGINE_ID_NORMAL_IE, false), 0);
        }
        /// <summary>
        /// 保護モードのIEからCookieを取得するICookieImporterを取得します。
        /// </summary>
        public ICookieImporter GetIEPMCookieImporter()
        {
            var cookieFolder = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Cookies), "low");
            return new IEPMCookieImporter(new BrowserConfig(
                "IE Protected", "Default", cookieFolder, ENGINE_ID_PROTECTED_IE, false), 0);
        }
        /// <summary>
        /// 拡張保護モードのIEからCookieを取得するICookieImporterを取得します。
        /// </summary>
        public ICookieImporter GetIEEPMCookieImporter()
        {
            var cookieFolder = Utility.ReplacePathSymbols(
                @"%LOCALAPPDATA%\Packages\windows_ie_ac_001\AC\INetCookies");
            return new IEFindCacheCookieImporter(new BrowserConfig(
                "IE Enhanced Protected", "Default", cookieFolder, ENGINE_ID_ENHANCED_PROTECTED_IE, false), 0);
        }

        internal const string ENGINE_ID_NORMAL_IE =
            "SunokoLibrary.Application.Browsers.IEBrowserManager.NormalIE";
        internal const string ENGINE_ID_PROTECTED_IE =
            "SunokoLibrary.Application.Browsers.IEBrowserManager.ProtectedIE";
        internal const string ENGINE_ID_ENHANCED_PROTECTED_IE =
            "SunokoLibrary.Application.Browsers.IEBrowserManager.EnhancedProtectedIE";
    }
}