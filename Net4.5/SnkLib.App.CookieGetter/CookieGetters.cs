using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
    public class CookieGetters : ICookieImporterManager
    {
        /// <summary>
        /// CookieGettersを生成します。引数省略時にはCookieGettersのImporterFactories、ImporterGeneratorsが使用されます。
        /// </summary>
        /// <param name="factories">対応させるブラウザ用のファクトリのシーケンス</param>
        /// <param name="generators">対応させるブラウザ用のジェネレータのシーケンス</param>
        /// <exception cref="ArgumentException">引数generatorsに同一のEngineIdを持つ要素が含まれている場合にスローされます。</exception>
        public CookieGetters(
            IEnumerable<ICookieImporterFactory> factories = null,
            IEnumerable<ICookieImporterGenerator> generators = null)
        {
            try
            {
                _factories = factories.ToArray() ?? ImporterFactories.ToArray();
                _generators = (generators ?? ImporterGenerators)
                    .SelectMany(item => item.EngineIds.Select(id => new { Importer = item, EngineId = id }))
                    .ToDictionary(item => item.EngineId, item => item.Importer);
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException(
                    "引数generators内に同一のEngineIdを持つ要素を含む事はできません。", e);
            }
        }
        ICookieImporterFactory[] _factories;
        Dictionary<string, ICookieImporterGenerator> _generators;

        /// <summary>
        /// 使用できるICookieImporterのリストを取得します。
        /// </summary>
        /// <param name="availableOnly">利用可能なものだけを出力するか指定します。</param>
        public Task<ICookieImporter[]> GetInstancesAsync(bool availableOnly)
        {
            return Task.Factory.StartNew(() => _factories
                .SelectMany(item => item.GetCookieImporters())
                .GroupBy(item => item.Config)
                .Select(grp => grp.First())
                .Where(item => item.IsAvailable || !availableOnly).ToArray());
        }
        /// <summary>
        /// 設定値を指定したICookieImporterを取得します。アプリ終了時に直前まで使用していた
        /// ICookieImporterのConfigを設定として保存すれば、起動時にConfigをこのメソッドに
        /// 渡す事で適切なICookieImporterを再取得する事ができます。
        /// </summary>
        /// <param name="targetConfig">再取得対象のブラウザの構成情報</param>
        /// <param name="allowDefault">取得不可の場合に既定のCookieImporterを返すかを指定できます。</param>
        public async Task<ICookieImporter> GetInstanceAsync(BrowserConfig targetConfig = null, bool allowDefault = true)
        {
            var foundGetter = null as ICookieImporter;
            var getterList = await GetInstancesAsync(false);

            if (targetConfig != null)
            {
                //引数targetConfigと同一のImporterを探す。
                //あればそのまま使う。なければ登録されたジェネレータから新たに生成する。
                foundGetter = getterList.FirstOrDefault(item => item.Config == targetConfig);
                ICookieImporterGenerator foundFactory;
                if (foundGetter == null && _generators.TryGetValue(targetConfig.EngineId, out foundFactory))
                    foundGetter = foundFactory.GetCookieImporter(targetConfig);
            }
            if (allowDefault && foundGetter == null)
                foundGetter = getterList.FirstOrDefault(importer => importer.IsAvailable);

            return foundGetter;
        }

        static CookieGetters()
        {
            ImporterFactories = new ConcurrentQueue<ICookieImporterFactory>(new ICookieImporterFactory[] {
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
            ImporterGenerators = new ConcurrentQueue<ICookieImporterGenerator>(new ICookieImporterGenerator[] {
                new IEBrowserManager(), new ChromiumBrowserManager(),
                new FirefoxBrowserManager(), new WebkitQtBrowserManager(),
            });
            Default = new CookieGetters(ImporterFactories, ImporterGenerators);
        }
        /// <summary>
        /// GetInstancesAsync(availableOnly)が使うFactoryを追加できます。
        /// </summary>
        public static ConcurrentQueue<ICookieImporterFactory> ImporterFactories { get; private set; }
        /// <summary>
        /// GetInstanceAsync(targetConfig, allowDefault)が使うGeneratorを追加できます。
        /// </summary>
        public static ConcurrentQueue<ICookieImporterGenerator> ImporterGenerators { get; private set; }
        /// <summary>
        /// 既定のCookieGettersを取得します。
        /// </summary>
        public static ICookieImporterManager Default { get; private set; }
    }
}