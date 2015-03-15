using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
#if !NET20
using System.Threading.Tasks;
#endif

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
        /// <exception cref="ArgumentException">引数generatorsに同一のEngineIdを持つ要素が含まれている場合にスローされます。</exception>
        public CookieGetters(IEnumerable<ICookieImporterFactory> factories = null)
        {
            try
            {
                _factoryList = (factories ?? _importerFactories).ToArray();
                _factoryDict = _factoryList
                    .SelectMany(item => item.EngineIds.Select(id => new { Importer = item, EngineId = id }))
                    .ToDictionary(item => item.EngineId, item => item.Importer);
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException(
                    "引数generators内に同一のEngineIdを持つ要素を含む事はできません。", e);
            }
        }
        ICookieImporterFactory[] _factoryList;
        Dictionary<string, ICookieImporterFactory> _factoryDict;

        /// <summary>
        /// 使用できるICookieImporterのリストを取得します。
        /// </summary>
        /// <param name="availableOnly">利用可能なものだけを出力するか指定します。</param>
        #region // GetInstancesAsync(bool)
#if NET20
        public ICookieImporter[] GetInstances(bool availableOnly)
        {
            return _factoryList
                .SelectMany(item => item.GetCookieImporters())
                .GroupBy(item => item.Config)
                .Select(grp => grp.First())
                .Where(item => item.IsAvailable || !availableOnly).ToArray();
        }
#else
        public Task<ICookieImporter[]> GetInstancesAsync(bool availableOnly)
        {
            return Task.Factory.StartNew(() => _factoryList
                .SelectMany(item => item.GetCookieImporters())
                .GroupBy(item => item.Config)
                .Select(grp => grp.First())
                .Where(item => item.IsAvailable || !availableOnly).ToArray());
        }
#endif
        #endregion

        /// <summary>
        /// 設定値を指定したICookieImporterを取得します。アプリ終了時に直前まで使用していた
        /// ICookieImporterのConfigを設定として保存すれば、起動時にConfigをこのメソッドに
        /// 渡す事で適切なICookieImporterを再取得する事ができます。
        /// </summary>
        /// <param name="targetConfig">再取得対象のブラウザの構成情報</param>
        /// <param name="allowDefault">取得不可の場合に既定のCookieImporterを返すかを指定できます。</param>
        #region // GetInstanceAsync(BrowserConfig, bool)
#if NET20
        public ICookieImporter GetInstance(BrowserConfig targetConfig = null, bool allowDefault = true)
        {
            var foundImporter = null as ICookieImporter;
            var importerList = GetInstances(false);

            if (targetConfig != null)
            {
                //引数targetConfigと同一のImporterを探す。
                //あればそのまま使う。なければ登録されたジェネレータから新たに生成する。
                foundImporter = importerList.FirstOrDefault(item => item.Config == targetConfig);
                ICookieImporterFactory foundFactory;
                if (foundImporter == null && _factoryDict.TryGetValue(targetConfig.EngineId, out foundFactory))
                {
                    foundImporter = foundFactory.GetCookieImporter(targetConfig);
                    foundImporter = foundImporter.IsAvailable ? foundImporter : null;
                }
            }
            if (allowDefault && foundImporter == null)
                foundImporter = importerList.FirstOrDefault(importer => importer.IsAvailable);

            return foundImporter;
        }
#else
        public async Task<ICookieImporter> GetInstanceAsync(BrowserConfig targetConfig = null, bool allowDefault = true)
        {
            var foundImporter = null as ICookieImporter;
            var importerList = await GetInstancesAsync(false);

            if (targetConfig != null)
            {
                //引数targetConfigと同一のImporterを探す。
                //あればそのまま使う。なければ登録されたジェネレータから新たに生成する。
                foundImporter = importerList.FirstOrDefault(item => item.Config == targetConfig);
                ICookieImporterFactory foundFactory;
                if (foundImporter == null && _factoryDict.TryGetValue(targetConfig.EngineId, out foundFactory))
                {
                    foundImporter = foundFactory.GetCookieImporter(targetConfig);
                    foundImporter = foundImporter.IsAvailable ? foundImporter : null;
                }
            }
            if (allowDefault && foundImporter == null)
                foundImporter = importerList.FirstOrDefault(importer => importer.IsAvailable);

            return foundImporter;
        }
#endif
        #endregion

        static CookieGetters()
        {
            _importerFactories = new ICookieImporterFactory[] {
                new IEImporterFactory(),
                new FirefoxImporterFactory(),
                new ChromeImporterFactory(),
                new OperaWebkitImporterFactory(),
                new ChromiumImporterFactory(),
                new LunascapeImporterFactory(),
                new MaxthonImporterFactory(),
                new SleipnirImporterFactory(),
                new TungstenImporterFactory(),
                new SmartBlinkBrowserManager(),
                new SmartGeckoBrowserManager(),
            };
            Default = CookieGetters.Create();
        }
        static ICookieImporterFactory[] _importerFactories;

        /// <summary>
        /// 既定のCookieGettersを取得します。
        /// </summary>
        public static ICookieImporterManager Default { get; private set; }
        /// <summary>
        /// ICookieImporterManagerの実装を生成します。
        /// </summary>
        /// <param name="includeDefault">既定のFactoryを含めるか</param>
        /// <param name="factories">追加で登録するFactory</param>
        public static ICookieImporterManager Create(bool includeDefault = true, params ICookieImporterFactory[] factories)
        {
            var res = includeDefault
                ? factories != null ? _importerFactories.Concat(factories) : _importerFactories
                : factories != null ? factories : Enumerable.Empty<ICookieImporterFactory>();
            return new CookieGetters(res);
        }
    }
}