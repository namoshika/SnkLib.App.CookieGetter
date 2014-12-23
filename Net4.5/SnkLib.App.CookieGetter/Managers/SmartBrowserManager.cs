using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SunokoLibrary.Application.Browsers
{
    /// <summary>
    /// 特定のファイル構造のパターンからブラウザを
    /// 見つけてICookieImporterを取得します。
    /// </summary>
    public abstract class SmartBrowserManager : ICookieImporterFactory
    {
        /// <summary>
        /// パターンを入力してインスタンスを生成します。
        /// </summary>
        /// <param name="searchTarget">検索する対象の名前</param>
        /// <param name="targetType">対象の種類</param>
        /// <param name="generator">対象を含むファイルパスからFactoryを生成するメソッド</param>
        public SmartBrowserManager(
            string searchTarget, PathType targetType, Func<string, string, ICookieImporterFactory> generator)
        {
            _searchTarget = searchTarget;
            _targetType = targetType;
            _generator = generator;
        }
        string _searchTarget;
        PathType _targetType;
        Func<string, string, ICookieImporterFactory> _generator;

#pragma warning disable 1591
        public IEnumerable<ICookieImporter> GetCookieImporters()
        {
            var browsers = AppDataFolders
                .SelectMany(appDataPath =>
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
                    
                    //製品フォルダを列挙
                    IEnumerable<string> tmp;
                    try{ tmp = Directory.EnumerateDirectories(appDataPath);}
                    catch (UnauthorizedAccessException)
                    { return Enumerable.Empty<Tuple<string, string>>(); }
                    catch (System.Security.SecurityException)
                    { return Enumerable.Empty<Tuple<string, string>>(); }
                    catch (IOException)
                    { return Enumerable.Empty<Tuple<string, string>>(); }
                    //中身を2階層まで探索
                    for (var i = 0; i < 2; i++)
                        tmp = tmp.SelectMany(childPath =>
                        {
                            try
                            {
                                return
                                    Path.GetFileName(childPath) == _searchTarget ? new string[] { childPath } :
                                    ExistsTarget(Path.Combine(childPath, _searchTarget)) ? new string[] { Path.Combine(childPath, _searchTarget) } :
                                    Directory.EnumerateDirectories(childPath);
                            }
                            catch (UnauthorizedAccessException)
                            { return Enumerable.Empty<string>(); }
                            catch (System.Security.SecurityException)
                            { return Enumerable.Empty<string>(); }
                            catch (IOException)
                            { return Enumerable.Empty<string>(); }
                        });
                    return tmp
                        .Where(path => Path.GetFileName(path) == _searchTarget)
                        .Select(path => Tuple.Create(appDataPath, path));
                })
                .Select(inf => _generator(inf.Item1, inf.Item2))
                .Where(factory => factory != null);

            return browsers.SelectMany(item => item.GetCookieImporters());
        }
#pragma warning restore 1591

        bool ExistsTarget(string targetPath)
        { return _targetType == PathType.File ? File.Exists(targetPath) : Directory.Exists(targetPath); }

        static SmartBrowserManager()
        {
            AppDataFolders = new[] {
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
            };
        }
        readonly static string[] AppDataFolders;
    }
    /// <summary>
    /// Chromium系列のブラウザからICookieImporterを取得します。
    /// </summary>
    public class SmartBlinkBrowserManager : SmartBrowserManager
    {
#pragma warning disable 1591
        public SmartBlinkBrowserManager()
            : base("User Data", PathType.Directory, (appDataPath, userDataPath) =>
                {
                    var appName = Path.GetDirectoryName(userDataPath
                        .Substring(appDataPath.Length))
                        .Split(new[] { "\\" }, StringSplitOptions.RemoveEmptyEntries)
                        .LastOrDefault();
                    return string.IsNullOrEmpty(appName) == false
                        ? new BlinkBrowserManager(appName, userDataPath) : null;
                }) { }
#pragma warning restore 1591
    }
    /// <summary>
    /// Gecko系列のブラウザからICookieImporterを取得します。
    /// </summary>
    public class SmartGeckoBrowserManager : SmartBrowserManager
    {
#pragma warning disable 1591
        public SmartGeckoBrowserManager()
            : base("profiles.ini", PathType.File, (appDataPath, userDataPath) =>
                {
                    var appName = Path.GetDirectoryName(userDataPath
                        .Substring(appDataPath.Length))
                        .Split(new []{"\\"}, StringSplitOptions.RemoveEmptyEntries)
                        .LastOrDefault();
                    return string.IsNullOrEmpty(appName) == false
                        ? new GeckoBrowserManager(appName, Path.GetDirectoryName(userDataPath)) : null;
                }) { }
#pragma warning restore 1591
    }
}
