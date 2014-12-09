using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SunokoLibrary.Application
{
    using SunokoLibrary.Application.Browsers;
    
    /// <summary>
    /// 使用可能なICookieImporterを提供します。
    /// </summary>
    public static class CookieGetters
    {
        static CookieGetters()
        {
            BrowserManagers = new ConcurrentQueue<ICookieImporterFactory>(new ICookieImporterFactory[] {
                new IEBrowserManager(),
                new FirefoxBrowserManager(),
                new GoogleChromeBrowserManager(),
                new OperaWebkitBrowserManager(),
                new ChromiumBrowserManager(),
                new LunascapeBrowserManager(),
                new MaxthonBrowserManager(),
                new SleipnirBrowserManager(),
                new TungstenBrowserManager(),
                new SmartBlinkBrowserManager(),
                new SmartGeckoBrowserManager(),
            });
        }
        
        /// <summary>
        /// 対応するブラウザのリスト
        /// </summary>
        public static ConcurrentQueue<ICookieImporterFactory> BrowserManagers { get; private set; }
        /// <summary>
        /// Cookie取得用インスタンスのリストを取得する
        /// </summary>
        /// <param name="availableOnly">利用可能なものだけを選択するかどうか</param>
        public static Task<ICookieImporter[]> GetInstancesAsync(bool availableOnly)
        {
            return Task.Run(() => BrowserManagers
                .SelectMany(item => item.GetCookieImporters())
                .GroupBy(item => item.Config)
                .Select(grp => grp.First())
                .Where(item => item.IsAvailable || !availableOnly).ToArray());
        }
        /// <summary>
        /// 設定値を復元したCookie取得用インスタンスを取得する。直前まで使用していたICookieImporterのConfigを保存しておいたりすると起動時に最適な既定値を選んでくれる。
        /// </summary>
        /// <param name="targetConfig">任意のブラウザ環境設定</param>
        /// <param name="allowDefault">生成不可の場合に既定のCookieImporterを返すか</param>
        public static async Task<ICookieImporter> GetInstanceAsync(BrowserConfig targetConfig = null, bool allowDefault = true)
        {
            var foundGetter = null as ICookieImporter;
            var getterList = await GetInstancesAsync(false);

            if (targetConfig != null)
            {
                //引数targetConfigと同一のブラウザを探す。
                //あればConfigを比較。同一ならばそのまま使う。違いがあればカスタマイズ版を生成する
                foundGetter = getterList.FirstOrDefault(item => item.Config.BrowserName == targetConfig.BrowserName);
                if (foundGetter != null && targetConfig != foundGetter.Config)
                    foundGetter = foundGetter.Generate(targetConfig.GenerateCopy());
            }
            if (allowDefault)
                foundGetter = foundGetter
                    ?? getterList.Where(importer => importer.IsAvailable).FirstOrDefault();

            return foundGetter;
        }
    }
}