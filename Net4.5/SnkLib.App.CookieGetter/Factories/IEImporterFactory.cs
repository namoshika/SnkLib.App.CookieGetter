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
        public override ICookieImporter GetCookieImporter(CookieSourceInfo sourceInfo)
        {
            switch(sourceInfo.EngineId)
            {
                case ENGINE_ID_NORMAL_IE:
                    return new IECookieImporter(sourceInfo, 2);
                case ENGINE_ID_PROTECTED_IE:
                    return new IEPMCookieImporter(sourceInfo, 2);
                case ENGINE_ID_ENHANCED_PROTECTED_IE:
                    return new IEFindCacheCookieImporter(sourceInfo, 2);
                default:
                    throw new ArgumentException("引数infoのEngineIdを使えるImporterが見つかりませんでした。");
            }
        }
#pragma warning restore 1591

        /// <summary>
        /// 非保護モードのIEからCookieを取得するICookieImporterを取得します。
        /// </summary>
        public ICookieImporter GetIECookieImporter()
        {
            if (_ieImporter == null)
            {
                var cookieFolder = Environment.GetFolderPath(Environment.SpecialFolder.Cookies);
                _ieImporter = new IECookieImporter(new CookieSourceInfo(
                    "IE Normal", "Default", cookieFolder, ENGINE_ID_NORMAL_IE, false), 0);
            }
            return _ieImporter;
        }
        /// <summary>
        /// 保護モードのIEからCookieを取得するICookieImporterを取得します。
        /// </summary>
        public ICookieImporter GetIEPMCookieImporter()
        {
            if (_iePMImporter == null)
            {
                var cookieFolder = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Cookies), "low");
                _iePMImporter = new IEPMCookieImporter(new CookieSourceInfo(
                    "IE Protected", "Default", cookieFolder, ENGINE_ID_PROTECTED_IE, false), 0);
            }
            return _iePMImporter;
        }
        /// <summary>
        /// 拡張保護モードのIEからCookieを取得するICookieImporterを取得します。
        /// </summary>
        public ICookieImporter GetIEEPMCookieImporter()
        {
            if (_ieEPMImporter == null)
            {
                var cookieFolder = Utility.ReplacePathSymbols(
                    @"%LOCALAPPDATA%\Packages\windows_ie_ac_001\AC\INetCookies");
                _ieEPMImporter = new IEFindCacheCookieImporter(new CookieSourceInfo(
                    "IE Enhanced Protected", "Default", cookieFolder, ENGINE_ID_ENHANCED_PROTECTED_IE, false), 0);
            }
            return _ieEPMImporter;
        }

        internal const string ENGINE_ID_NORMAL_IE =
            "SunokoLibrary.Application.Browsers.IEBrowserManager.NormalIE";
        internal const string ENGINE_ID_PROTECTED_IE =
            "SunokoLibrary.Application.Browsers.IEBrowserManager.ProtectedIE";
        internal const string ENGINE_ID_ENHANCED_PROTECTED_IE =
            "SunokoLibrary.Application.Browsers.IEBrowserManager.EnhancedProtectedIE";
        static ICookieImporter _ieImporter;
        static ICookieImporter _iePMImporter;
        static ICookieImporter _ieEPMImporter;
    }
}