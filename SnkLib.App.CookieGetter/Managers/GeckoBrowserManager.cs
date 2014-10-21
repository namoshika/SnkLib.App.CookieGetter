﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SunokoLibrary.Application.Browsers
{
    public class GeckoBrowserManager : ICookieImporterFactory
    {
        public GeckoBrowserManager(Func<BrowserConfig, ICookieImporter> getterGenerator,
            string name = null, string dataFolder = null,
            string cookieFileName = "cookies.sqlite", string iniFileName = "profiles.ini")
        {
            _getterGenerator = getterGenerator;
            Name = name;
            DataFolder = dataFolder != null ? Utility.ReplacePathSymbols(dataFolder) : null;
            IniFileName = iniFileName;
            CookieFileName = cookieFileName;
        }
        Func<BrowserConfig, ICookieImporter> _getterGenerator;
        protected string Name;
        protected string DataFolder;
        protected string IniFileName;
        protected string CookieFileName;

        public IEnumerable<ICookieImporter> GetCookieImporters()
        {
            var getters = UserProfile.GetProfiles(DataFolder, IniFileName)
                .Select(prof => new BrowserConfig(Name, prof.Name, Path.Combine(prof.Path, CookieFileName)))
                .Select(inf => _getterGenerator(inf))
                .ToArray();
            getters = getters.Length == 0
                ? new ICookieImporter[] { _getterGenerator(new BrowserConfig(Name, "None", null)) } : getters;
            return getters;
        }

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
