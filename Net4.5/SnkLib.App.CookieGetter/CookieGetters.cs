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
        /// CookieGettersを生成します。
        /// </summary>
        /// <param name="includeDefault">既定のFactoryを含めるか</param>
        /// <param name="factories">追加で登録するFactory</param>
        /// <exception cref="ArgumentException">引数factoriesに同一のEngineIdを持つ要素が含まれている場合にスローされます。</exception>
        public CookieGetters(bool includeDefault = true, params ICookieImporterFactory[] factories)
        {
            var args = includeDefault
                ? factories != null ? _importerFactories.Concat(factories) : _importerFactories
                : factories != null ? factories : Enumerable.Empty<ICookieImporterFactory>();
            _factoryList = args.ToArray();
            try
            {
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
        public Task<ICookieImporter[]> GetInstancesAsync(bool availableOnly)
        {
            return Task.Factory.StartNew(() => _factoryList
                .SelectMany(item => item.GetCookieImporters())
                .GroupBy(item => item.SourceInfo)
                .Select(grp => grp.First())
                .Where(item => item.IsAvailable || !availableOnly).ToArray());
        }

        /// <summary>
        /// 設定値を指定したICookieImporterを取得します。アプリ終了時に直前まで使用していた
        /// ICookieImporterのSourceInfoを設定として保存すれば、起動時にSourceInfoをこのメソッドに
        /// 渡す事で適切なICookieImporterを再取得する事ができます。
        /// </summary>
        /// <param name="targetInfo">再取得対象のブラウザの構成情報</param>
        /// <param name="allowDefault">取得不可の場合に既定のCookieImporterを返すかを指定できます。</param>
        public async Task<ICookieImporter> GetInstanceAsync(CookieSourceInfo targetInfo = null, bool allowDefault = true)
        {
            var foundImporter = null as ICookieImporter;
            var importerList = await GetInstancesAsync(false);

            if (targetInfo != null)
            {
                //引数targetInfoと同一のImporterを探す。
                //あればそのまま使う。なければ登録されたジェネレータから新たに生成する。
                foundImporter = importerList.FirstOrDefault(item => item.SourceInfo == targetInfo);
                ICookieImporterFactory foundFactory;
                if (foundImporter == null && _factoryDict.TryGetValue(targetInfo.EngineId, out foundFactory))
                {
                    foundImporter = foundFactory.GetCookieImporter(targetInfo);
                    foundImporter = foundImporter.IsAvailable ? foundImporter : null;
                }
            }
            if (allowDefault && foundImporter == null)
                foundImporter = importerList.FirstOrDefault(importer => importer.IsAvailable);

            return foundImporter;
        }

        static CookieGetters()
        {
            _importerFactories = new ICookieImporterFactory[] {
                _ieFactory, _egFactory, _ffFactory, _chFactory,
                new OperaWebkitImporterFactory(),
                new ChromiumImporterFactory(),
                new LunascapeImporterFactory(),
                new MaxthonImporterFactory(),
                new SleipnirImporterFactory(),
                new TungstenImporterFactory(),
                new SmartBlinkBrowserManager(),
                new SmartGeckoBrowserManager(),
            };
            Default = new CookieGetters();
        }
        static ICookieImporterFactory[] _importerFactories;
        static IEImporterFactory _ieFactory = new IEImporterFactory();
        static EdgeImporterFactory _egFactory = new EdgeImporterFactory();
        static ChromeImporterFactory _chFactory = new ChromeImporterFactory();
        static FirefoxImporterFactory _ffFactory = new FirefoxImporterFactory();
        static ICookieImporter _chImporter;
        static ICookieImporter _ffImporter;
        static ICookieImporter _egImporter;

        /// <summary>
        /// 既定のCookieGettersを取得します。
        /// </summary>
        public static ICookieImporterManager Default { get; private set; }
        /// <summary>
        /// ブラウザの既定ICookieImporterを提供します。
        /// </summary>
        public static class Browsers
        {
            /// <summary>
            /// 通常モードのIEのICookieImporterを取得します。
            /// </summary>
            public static ICookieImporter IENormal
            { get { return _ieFactory.GetIECookieImporter(); } }
            /// <summary>
            /// 保護モードのIEのICookieImporterを取得します。
            /// </summary>
            public static ICookieImporter IEProtected
            { get { return _ieFactory.GetIEPMCookieImporter(); } }
            /// <summary>
            /// 拡張保護モードのIEのICookieImporterを取得します。
            /// </summary>
            public static ICookieImporter IEEnhancedProtected
            { get { return _ieFactory.GetIEEPMCookieImporter(); } }
            /// <summary>
            /// MicrosoftEdgeのICookieImporterを取得します。
            /// </summary>
            public static ICookieImporter MicrosoftEdge
            {
                get
                {
                    if (_egImporter == null)
                        _egImporter = _egFactory.GetCookieImporters().FirstOrDefault();
                    return _egImporter;
                }
            }
            /// <summary>
            /// FirefoxのICookieImporterを取得します。
            /// </summary>
            public static ICookieImporter Firefox
            {
                get
                {
                    if (_ffImporter == null)
                        _ffImporter = _ffFactory.GetCookieImporters().FirstOrDefault();
                    return _ffImporter;
                }
            }
            /// <summary>
            /// ChromeのICookieImporterを取得します。
            /// </summary>
            public static ICookieImporter Chrome
            {
                get
                {
                    if (_chImporter == null)
                        _chImporter = _chFactory.GetCookieImporters().FirstOrDefault();
                    return _chImporter;
                }
            }
        }
    }
}