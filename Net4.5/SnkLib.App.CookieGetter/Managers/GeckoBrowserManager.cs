using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SunokoLibrary.Application.Browsers
{
    public class GeckoBrowserManager : BrowserManagerBase
    {
        public GeckoBrowserManager(
            string name, string dataFolder, int primaryLevel = 2,
            string cookieFileName = "cookies.sqlite", string iniFileName = "profiles.ini")
            : base(new[] { ENGINE_ID })
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("引数nameをnullや空文字にする事は出来ません。");

            _name = name;
            _primaryLevel = primaryLevel;
            _dataFolder = dataFolder != null ? Utility.ReplacePathSymbols(dataFolder) : null;
            _iniFileName = iniFileName;
            _cookieFileName = cookieFileName;
        }

        internal const string ENGINE_ID = "Gecko";
        int _primaryLevel;
        string _name;
        string _dataFolder;
        string _iniFileName;
        string _cookieFileName;
        public override IEnumerable<ICookieImporter> GetCookieImporters()
        {
            var getters = UserProfile.GetProfiles(_dataFolder, _iniFileName)
                .Select(prof => new BrowserConfig(_name, prof.Name, Path.Combine(prof.Path, _cookieFileName), ENGINE_ID, false))
                .Select(inf => (ICookieImporter)new GeckoCookieGetter(inf, _primaryLevel))
                .ToArray();
            getters = getters.Length == 0
                ? new ICookieImporter[] { new GeckoCookieGetter(new BrowserConfig(_name, "Default", null, ENGINE_ID, false), _primaryLevel) } : getters;
            return getters;
        }
        public override ICookieImporter GetCookieImporter(BrowserConfig config)
        { return new GeckoCookieGetter(config, 2); }

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
            /// Firefoxのプロフィールフォルダ内のフォルダをすべて取得する
            /// </summary>
            /// <returns></returns>
            public static UserProfile[] GetProfiles(string moz_path, string iniFileName)
            {
                var profileListPath = System.IO.Path.Combine(moz_path, iniFileName);
                var results = new List<UserProfile>();
                if (File.Exists(profileListPath) == false)
                    return results.ToArray();

                UserProfile prof = null;
                using (var sr = new StreamReader(profileListPath))
                    while (!sr.EndOfStream)
                    {
                        var line = sr.ReadLine();
                        if (line.StartsWith("[Profile"))
                        {
                            prof = new UserProfile();
                            results.Add(prof);
                        }
                        if (prof != null)
                        {
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
