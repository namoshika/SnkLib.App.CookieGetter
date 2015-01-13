using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SunokoLibrary.Application.Browsers
{
    /// <summary>
    /// Chromium系列のブラウザからICookieImporterを取得する基盤クラス
    /// </summary>
    public class BlinkImporterFactory : ImporterFactoryBase
    {
        /// <summary>
        /// 指定したブラウザ情報でインスタンスを生成します。
        /// </summary>
        /// <param name="name">ブラウザ名</param>
        /// <param name="dataFolder">UserDataのフォルダパス</param>
        /// <param name="primaryLevel">ブラウザの格</param>
        /// <param name="cookieFileName">Cookieファイルの名前</param>
        /// <param name="defaultFolder">デフォルトのプロファイルフォルダの名前</param>
        /// <param name="profileFolderStarts">デフォルト以外のプロファイルフォルダの名前のプレフィックス</param>
        /// <param name="engineId">エンジン識別子</param>
        public BlinkImporterFactory(
            string name, string dataFolder, int primaryLevel = 2, string cookieFileName = "Cookies",
            string defaultFolder = "Default", string profileFolderStarts = "Profile", string engineId = null)
            : base(engineId != null ? new[] { engineId } : null)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("引数nameをnullや空文字にする事は出来ません。");

            _name = name;
            _dataFolder = dataFolder != null ? Utility.ReplacePathSymbols(dataFolder) : null;
            _primaryLevel = primaryLevel;
            _cookieFileName = cookieFileName;
            _defaultFolderName = defaultFolder;
            _profileFolderStarts = profileFolderStarts;
        }

#pragma warning disable 1591

        int _primaryLevel;
        string _name;
        string _dataFolder;
        string _cookieFileName;
        string _defaultFolderName;
        string _profileFolderStarts;

        public override IEnumerable<ICookieImporter> GetCookieImporters()
        { return GetDefaultProfiles().Concat(GetProfiles()); }
        public override ICookieImporter GetCookieImporter(BrowserConfig config)
        { return new BlinkCookieImporter(config, 2); }

#pragma warning restore 1591

        /// <summary>
        /// ユーザのデフォルト環境設定を用いたICookieImporter生成します。
        /// </summary>
        /// <returns>長さ1の列挙子</returns>
        IEnumerable<ICookieImporter> GetDefaultProfiles()
        {
            string path = null;
            if (_dataFolder != null)
                path = Path.Combine(_dataFolder, _defaultFolderName, _cookieFileName);
            var conf = new BrowserConfig(_name, _defaultFolderName, path, EngineIds[0], false);
            return new ICookieImporter[] { new BlinkCookieImporter(conf, _primaryLevel) };
        }
        /// <summary>
        /// ブラウザが持っているデフォルト以外の全ての環境設定からICookieImporterを生成します。
        /// </summary>
        IEnumerable<ICookieImporter> GetProfiles()
        {
            var paths = Enumerable.Empty<ICookieImporter>();
            if (Directory.Exists(_dataFolder))
            {
                paths = Directory.EnumerateDirectories(_dataFolder)
                    .Where(path => Path.GetFileName(path).StartsWith(_profileFolderStarts, StringComparison.OrdinalIgnoreCase))
                    .Select(path => Path.Combine(path, _cookieFileName))
                    .Where(path => File.Exists(path))
                    .Select(path => new BlinkCookieImporter(new BrowserConfig(
                        _name, Path.GetFileName(Path.GetDirectoryName(path)), path, EngineIds[0], false), _primaryLevel));
                return paths;
            }
            return paths;
        }
    }
}