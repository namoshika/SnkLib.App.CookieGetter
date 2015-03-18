using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace SunokoLibrary.Application.Browsers
{
    /// <summary>
    /// 特定のファイル構造のパターンからブラウザを
    /// 見つけてICookieImporterを取得します。
    /// </summary>
    public abstract class SmartImporterFactory : ImporterFactoryBase
    {
        /// <summary>
        /// パターンを入力してインスタンスを生成します。
        /// </summary>
        /// <param name="searchTarget">検索する対象の名前</param>
        /// <param name="targetType">対象の種類</param>
        public SmartImporterFactory(string searchTarget, CookiePathType targetType)
        {
            _searchTarget = searchTarget;
            _targetType = targetType;
        }
        string _searchTarget;
        CookiePathType _targetType;

#pragma warning disable 1591
        public override IEnumerable<ICookieImporter> GetCookieImporters()
        {
            var browsers = AppDataFolders
                .SelectMany(appDataPath =>
                    FindTargets(appDataPath, _searchTarget, _targetType)
                        .Select(entryPath => new { AppDataPath = appDataPath, EntryPath = entryPath }))
                .Select(inf => Generate(inf.AppDataPath, inf.EntryPath, EngineIds[0]))
                .Where(factory => factory != null);

            return browsers.SelectMany(item => item.GetCookieImporters());
        }
        public abstract override ICookieImporter GetCookieImporter(CookieSourceInfo sourceInfo);
        protected abstract ICookieImporterFactory Generate(string appDataPath, string userDataPath, string engineId);
#pragma warning restore 1591

        static SmartImporterFactory()
        {
            AppDataFolders = new[] {
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
            };
        }
        static IEnumerable<string> FindTargets(string dirPath, string targetName, CookiePathType fileType)
        {
            /* 中身を探索し、ユーザデータを探す。
             * ガイドライン的にフォルダは以下の構造になっていると予想される。
             * > (%APPDATA%|%LOCALAPPDATA%)\(ProductName\){1,3}User Data\(ProfileName)\Cookies etc..
             * > 参考文献: http://d.hatena.ne.jp/torutk/20110604/p1
             * 
             * そのため、探索は製品フォルダから2階層下まで見る。
             * ユーザデータを発見したらその中身は探索しない。しかし、同一ベンダ、同一製品で複数の
             * 系統というのは存在しうるため、兄弟フォルダなどの探索は継続する。
             */
            var searchingPaths = new Stack<string>(new[] { dirPath });
            var searchingLevels = new Stack<int>(new[] { 0 });
            while (searchingPaths.Count > 0)
            {
                var itemLevel = searchingLevels.Pop();
                var itemLongPath = searchingPaths.Pop();
                var itemShrtPath = itemLongPath.Substring(dirPath.Length);
                var targetFilePath = Path.Combine(itemLongPath, targetName);

                if (fileType == CookiePathType.File ? File.Exists(targetFilePath) : Directory.Exists(targetFilePath))
                {
                    yield return targetFilePath;
                    continue;
                }
                //中身を2階層まで探索
                //"AppData\Vender\Product\Edition"で4つ分になる。
                if (itemLevel > 3)
                    continue;

                //製品フォルダを列挙
                IEnumerable<string> tmp;
                try { tmp = Directory.EnumerateDirectories(itemLongPath); }
                catch (UnauthorizedAccessException) { continue; }
                catch (System.Security.SecurityException) { continue; }
                catch (IOException) { continue; }

                foreach (var item in tmp)
                {
                    searchingPaths.Push(item);
                    searchingLevels.Push(itemLevel + 1);
                }
            }
        }
        readonly static string[] AppDataFolders;
    }
    /// <summary>
    /// Chromium系列のブラウザからICookieImporterを取得します。
    /// </summary>
    public class SmartBlinkBrowserManager : SmartImporterFactory
    {
#pragma warning disable 1591
        public SmartBlinkBrowserManager() : base("User Data", CookiePathType.Directory) { }
        public override ICookieImporter GetCookieImporter(CookieSourceInfo sourceInfo)
        { return new BlinkCookieImporter(sourceInfo, 2); }
        protected override ICookieImporterFactory Generate(string appDataPath, string userDataPath, string engineId)
        {
            var appName = Path.GetDirectoryName(userDataPath
                .Substring(appDataPath.Length))
                .Split(new[] { "\\" }, StringSplitOptions.RemoveEmptyEntries)
                .LastOrDefault();
            return string.IsNullOrEmpty(appName) == false
                ? new BlinkImporterFactory(appName, userDataPath, engineId: engineId) : null;
        }
#pragma warning restore 1591
    }
    /// <summary>
    /// Gecko系列のブラウザからICookieImporterを取得します。
    /// </summary>
    public class SmartGeckoBrowserManager : SmartImporterFactory
    {
#pragma warning disable 1591
        public SmartGeckoBrowserManager() : base("profiles.ini", CookiePathType.File) { }
        public override ICookieImporter GetCookieImporter(CookieSourceInfo sourceInfo)
        { return new GeckoCookieImporter(sourceInfo, 2); }
        protected override ICookieImporterFactory Generate(string appDataPath, string userDataPath, string engineId)
        {
            var appName = Path.GetDirectoryName(userDataPath
                .Substring(appDataPath.Length))
                .Split(new[] { "\\" }, StringSplitOptions.RemoveEmptyEntries)
                .LastOrDefault();
            return string.IsNullOrEmpty(appName) == false
                ? new GeckoImporterFactory(appName, Path.GetDirectoryName(userDataPath), engineId: engineId) : null;
        }
#pragma warning restore 1591
    }
}