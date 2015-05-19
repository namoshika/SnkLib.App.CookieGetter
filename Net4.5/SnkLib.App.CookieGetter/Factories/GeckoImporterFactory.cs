using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace SunokoLibrary.Application.Browsers
{
    /// <summary>
    /// Firefox系列のブラウザからICookieImporterを取得する基盤クラス
    /// </summary>
    public class GeckoImporterFactory : ImporterFactoryBase
    {
        /// <summary>
        /// 指定したブラウザ情報でインスタンスを生成します。
        /// </summary>
        /// <param name="name">ブラウザ名</param>
        /// <param name="dataFolder">対象のブラウザ用の設定フォルダパス</param>
        /// <param name="primaryLevel">ブラウザの格</param>
        /// <param name="cookieFileName">Cookieファイルの名前</param>
        /// <param name="iniFileName">設定ファイルの名前</param>
        /// <param name="engineId">エンジン識別子</param>
        public GeckoImporterFactory(
            string name, string dataFolder, int primaryLevel = 2,
            string cookieFileName = "cookies.sqlite", string iniFileName = "profiles.ini", string engineId = null)
            : base(engineId != null ? new[] { engineId } : null)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("引数nameをnullや空文字にする事は出来ません。");

            _name = name;
            _primaryLevel = primaryLevel;
            _dataFolder = dataFolder != null ? Utility.ReplacePathSymbols(dataFolder) : null;
            _iniFileName = iniFileName;
            _cookieFileName = cookieFileName;
        }
        int _primaryLevel;
        string _name;
        string _dataFolder;
        string _iniFileName;
        string _cookieFileName;

#pragma warning disable 1591
        public override IEnumerable<ICookieImporter> GetCookieImporters()
        {
            var importers = UserProfile.GetProfiles(_dataFolder, _iniFileName)
                .Select(prof => new CookieSourceInfo(_name, prof.Name, Path.Combine(prof.Path, _cookieFileName), EngineIds[0], false))
                .Select(inf => (ICookieImporter)new GeckoCookieImporter(inf, _primaryLevel))
                .ToArray();
            importers = importers.Length == 0
                ? new ICookieImporter[] { new GeckoCookieImporter(new CookieSourceInfo(_name, "Default", null, EngineIds[0], false), _primaryLevel) } : importers;
            return importers;
        }
        public override ICookieImporter GetCookieImporter(CookieSourceInfo sourceInfo)
        { return new GeckoCookieImporter(sourceInfo, 2); }
#pragma warning restore 1591

        /// <summary>
        /// ユーザの環境設定。ブラウザが複数の環境設定を持てる場合に使う。
        /// </summary>
        class UserProfile
        {
            public string Name;
            public bool IsRelative;
            public string Path;
            public bool IsDefault;

            /// <summary>
            /// Firefoxのプロフィールフォルダ内のフォルダをすべて取得します。
            /// </summary>
            /// <returns></returns>
            public static UserProfile[] GetProfiles(string moz_path, string iniFileName)
            {
                var profileListPath = System.IO.Path.Combine(moz_path, iniFileName);
                var results = new List<UserProfile>();
                if (File.Exists(profileListPath) == false)
                    return results.ToArray();

                using (var sr = new StreamReader(profileListPath))
                {
                    //セクション毎ループ
                    var line = null as string;
                    var recheck = false;
                    while (!sr.EndOfStream || recheck)
                    {
                        //行再処理フラグが立っていなければ行を進める
                        if (recheck)
                            recheck = false;
                        else
                            line = sr.ReadLine();

                        if (line.StartsWith("[Profile"))
                        {
                            //設定値毎ループ
                            var prof = new UserProfile();
                            while (!sr.EndOfStream)
                            {
                                //lineBが"["から始まっている場合、
                                //前のセクション終了して次のセクションが開始した事を示す。
                                //新セクションが開始したら何もせずに外ループからやり直す。
                                line = sr.ReadLine();
                                if (line.StartsWith("["))
                                {
                                    recheck = true;
                                    break;
                                }
                                var pair = ParseKeyValuePair(line);
                                switch (pair.Key)
                                {
                                    case "Name":
                                        prof.Name = pair.Value;
                                        break;
                                    case "IsRelative":
                                        prof.IsRelative = pair.Value == "1";
                                        break;
                                    case "Path":
                                        prof.Path = pair.Value.Replace('/', '\\');
                                        if (prof.IsRelative)
                                            prof.Path = System.IO.Path.Combine(moz_path, prof.Path);
                                        break;
                                    case "Default":
                                        prof.IsDefault = pair.Value == "1";
                                        break;
                                }
                            }
                            if (string.IsNullOrEmpty(prof.Path) == false)
                                if (prof.IsDefault)
                                    results.Insert(0, prof);
                                else
                                    results.Add(prof);
                        }
                    }
                }
                return results.ToArray();
            }
            static KeyValuePair<string, string> ParseKeyValuePair(string line)
            {
                string[] x = line.Split('=');
                return x.Length == 2
                    ? new KeyValuePair<string, string>(x[0], x[1])
                    : new KeyValuePair<string, string>();
            }
        }
    }
}
