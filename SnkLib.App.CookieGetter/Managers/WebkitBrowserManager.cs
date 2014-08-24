using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SunokoLibrary.Application.Browsers
{
    public class WebkitBrowserManager : ICookieImporterFactory
    {
        public WebkitBrowserManager(Func<BrowserConfig, ICookieImporter> getterGenerator,
            string name = null, string dataFolder = null, string cookieFileName = "Cookies",
            string defaultFolder = "Default", string profileFolderStarts = "Profile")
        {
            _getterGenerator = getterGenerator;
            Name = name;
            DataFolder = dataFolder != null ? Utility.ReplacePathSymbols(dataFolder) : null;
            CookieFileName = cookieFileName;
            DefaultFolderName = defaultFolder;
            ProfileFolderStarts = profileFolderStarts;
        }
        Func<BrowserConfig, ICookieImporter> _getterGenerator;
        protected string Name;
        protected string DataFolder;
        protected string CookieFileName;
        protected string DefaultFolderName;
        protected string ProfileFolderStarts;

        public ICookieImporter[] CreateCookieImporters()
        {
            return GetDefaultProfiles(_getterGenerator)
                .Concat(GetProfiles(_getterGenerator)).ToArray();
        }
        /// <summary>
        /// ユーザのデフォルト環境設定を用いたICookieImporter生成。
        /// </summary>
        /// <param name="getterGenerator">configを任意のimporterに変換する</param>
        /// <returns>長さ1の列挙子</returns>
        IEnumerable<ICookieImporter> GetDefaultProfiles(Func<BrowserConfig, ICookieImporter> getterGenerator)
        {
            string path = null;
            if (DataFolder != null)
                path = Path.Combine(DataFolder, DefaultFolderName, CookieFileName);
            var option = new BrowserConfig(Name, DefaultFolderName, path);
            return new ICookieImporter[] { getterGenerator(option) };
        }
        /// <summary>
        /// ブラウザが持っているデフォルト以外の全ての環境設定からICookieImporterを生成する。
        /// </summary>
        /// <param name="getterGenerator">configを任意のimporterに変換する</param>
        /// <returns></returns>
        IEnumerable<ICookieImporter> GetProfiles(Func<BrowserConfig, ICookieImporter> getterGenerator)
        {
            var paths = Enumerable.Empty<ICookieImporter>();
            if (Directory.Exists(DataFolder))
            {
                paths = Directory.EnumerateDirectories(DataFolder)
                    .Where(path => Path.GetFileName(path).StartsWith(ProfileFolderStarts, StringComparison.OrdinalIgnoreCase))
                    .Select(path => Path.Combine(path, CookieFileName))
                    .Where(path => File.Exists(path))
                    .Select(path => getterGenerator(
                        new BrowserConfig(Name, Path.GetFileName(Path.GetDirectoryName(path)), path)));
                return paths;
            }
            return paths;
        }
    }
}
