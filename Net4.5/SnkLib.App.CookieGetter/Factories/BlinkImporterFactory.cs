using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
#if !NET20
using Newtonsoft.Json.Linq;
#endif

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
        /// <param name="stateFileName">ブラウザの設定ファイルの名前</param>
        /// <param name="engineId">エンジン識別子</param>
        public BlinkImporterFactory(
            string name, string dataFolder, int primaryLevel = 2, string cookieFileName = "Cookies",
            string defaultFolder = "Default", string profileFolderStarts = "Profile",
            string stateFileName = "Local State", string engineId = null)
            : base(engineId != null ? new[] { engineId } : null)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("引数nameをnullや空文字にする事は出来ません。");

            _name = name;
            _dataFolder = dataFolder != null ? Utility.ReplacePathSymbols(dataFolder) : null;
            _primaryLevel = primaryLevel;
            _stateFileName = stateFileName;
            _cookieFileName = cookieFileName;
            _defaultFolderName = defaultFolder;
            _profileFolderStarts = profileFolderStarts;
        }

#pragma warning disable 1591
        int _primaryLevel;
        string _name;
        string _dataFolder;
        string _stateFileName;
        string _cookieFileName;
        string _defaultFolderName;
        string _profileFolderStarts;

        public override IEnumerable<ICookieImporter> GetCookieImporters()
        { return GetDefaultProfiles().Concat(GetProfiles()); }
        public override ICookieImporter GetCookieImporter(CookieSourceInfo sourceInfo)
        { return new BlinkCookieImporter(sourceInfo, 2); }
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
            var conf = new CookieSourceInfo(_name, _defaultFolderName, path, EngineIds[0], false);
            return new ICookieImporter[] { new BlinkCookieImporter(conf, _primaryLevel) };
        }
        /// <summary>
        /// ブラウザが持っているデフォルト以外の全ての環境設定からICookieImporterを生成します。
        /// </summary>
        IEnumerable<ICookieImporter> GetProfiles()
        {
            if (!Directory.Exists(_dataFolder))
                return Enumerable.Empty<ICookieImporter>();

            string stateTxt;
            try { stateTxt = File.ReadAllText(Path.Combine(_dataFolder, _stateFileName)); }
            catch (IOException) { return Enumerable.Empty<ICookieImporter>(); }

            var stateJson = (JObject)JToken.Parse(stateTxt);
            var paths = stateJson
                .Cast<JProperty>()
                .Where(item => item.Name == "profile").Take(1).SelectMany(item => item.Value)
                .Cast<JProperty>()
                .Where(item => item.Name == "info_cache").Take(1).SelectMany(item => item.Value)
                .Cast<JProperty>()
                .Where(item => item.Name != "Default")
                .Select(item =>
                    new
                    {
                        ProfName = (string)item.Value["name"],
                        CookiePath = Path.Combine(_dataFolder, item.Name, _cookieFileName)
                    })
                .Where(item => File.Exists(item.CookiePath))
                .Select(item => (ICookieImporter)new BlinkCookieImporter(new CookieSourceInfo(
                    _name, item.ProfName, item.CookiePath, EngineIds[0], false), _primaryLevel));
            return paths;
        }
    }
}