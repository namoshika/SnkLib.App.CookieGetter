using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace SunokoLibrary.Application
{
    using SunokoLibrary.Application.Browsers;
    
    /// <summary>
    /// 使用可能なCookieGetterを提供します。
    /// </summary>
    public static class CookieGetters
    {
        static CookieGetters()
        {
            BrowserManagers = new List<ICookieImporterFactory>(new ICookieImporterFactory[] {
                new IEBrowserManager(),
                new FirefoxBrowserManager(),
                new PaleMoonBrowserManager(),
                new SeaMonkeyBrowserManager(),
                new GoogleChromeBrowserManager(),
                new ComodoDragonBrowserManager(),
                new ComodoIceDragonBrowserManager(),
                new OperaWebkitBrowserManager(),
                new LunascapeGeckoBrowserManager(),
                new LunascapeWebkitBrowserManager(),
                new Sleipnir4BlinkBrowserManager(),
                new Sleipnir5BlinkBrowserManager(),
                new ChromiumBrowserManager(),
                new CoolNovoBrowserManager(),
                new MaxthonBrowserManager(),
                new TungstenBrowserManager()
            });
        }
        
        /// <summary>
        /// 登録されているCookieGetterのリスト
        /// </summary>
        public static List<ICookieImporterFactory> BrowserManagers { get; private set; }
        /// <summary>
        /// すべてのクッキーゲッターを取得する
        /// </summary>
        /// <param name="availableOnly">利用可能なものだけを選択するかどうか</param>
        public static IEnumerable<ICookieImporter> CreateInstances(bool availableOnly)
        {
            return BrowserManagers
                .SelectMany(item => item.CreateCookieImporters())
                .Where(item => !availableOnly || item.IsAvailable);
        }
        /// <summary>
        /// 設定値復元用。直前まで使用していたCookieGetterのConfigを保存しておいたりすると起動時に最適な既定値を選んでくれる。
        /// </summary>
        /// <param name="targetConfig">任意のブラウザ環境設定</param>
        /// <param name="allowDefault">生成不可の場合に既定のCookieImporterを返すか</param>
        public static ICookieImporter CreateInstance(BrowserConfig targetConfig, bool allowDefault = true)
        {
            var getterList = CreateInstances(false).ToArray();
            ICookieImporter foundGetter = null;
            if (targetConfig != null
                && string.IsNullOrEmpty(targetConfig.BrowserName) == false
                && string.IsNullOrEmpty(targetConfig.ProfileName) == false
                && string.IsNullOrEmpty(targetConfig.CookiePath) == false)
            {
                //使えそうなICookieImporterを探して、見つかったら保持しているConfigと差分の有無を比較。
                //違いが無いならば標準のを返す。有るならばconfig.Generate()でIsCustomizedをtrueにした
                //上で新しくICookieImporterを生成したものを返す。
                foundGetter = getterList.FirstOrDefault(item => item.Config.BrowserName == targetConfig.BrowserName);
                if (foundGetter != null && targetConfig != foundGetter.Config)
                    foundGetter = foundGetter.Generate(targetConfig.GenerateCopy());
            }
            if (allowDefault)
                foundGetter = foundGetter ?? CreateInstances(true).FirstOrDefault();

            return foundGetter;
        }
    }
}