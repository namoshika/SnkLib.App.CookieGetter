using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    using SunokoLibrary.Application;
    using SunokoLibrary.Application.Browsers;

    [TestClass]
    public class SimpleTests
    {
        const string REGRESSION_TESTDATA_PATH = "..\\..\\Datas";
        const string SQLITE_QUERY_CREATE_TABLE_COOKIES = "create table cookies(creation_utc integer, host_key text, name text, value text, path text, expires_utc integer, secure integer, httponly integer, last_access_utc integer, has_expires integer, persistent integer, priority integer, encrypted_value blob)";
        const string SQLITE_QUERY_CREATE_TABLE_META = "create table meta(key text, value text);";
        static string[] SqliteInsertQueries = new[]
            {
                "insert into cookies values(10001,'.nicovideo.jp', 'user_session', 'session_text10002', '/', {0}, 1, 1, 10006, 10007, 10008, 10009, @encryptData);",
                "insert into cookies values(20001,'.nicovideo.jp', 'nicorepo_filter', 'nicorepo_text20002', '/', {0}, 0, 0, 20006, 20007, 20008, 20009, @encryptData);",
                "insert into cookies values(30001,'.hoge.jp', 'nameAAA', 'valueAAA30003', '/', {0}, 0, 0, 30006, 30007, 30008, 30009, @encryptData);",
                "insert into cookies values(40001,'.foo.hoge.jp', 'nameBBB', 'valueBBB40002', '/', {0}, 0, 0, 40006, 40007, 40008, 40009, @encryptData);",
                "insert into meta values('version', '7');",
                "insert into meta values('last_compatible_version', '5');",
            };
        public static TestContext Context { get; private set; }
        public static System.IO.StreamWriter LogWriter;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            Context = context;

            //各ブラウザ向けImporterの動作確認結果を一覧として出力させる。
            LogWriter = new System.IO.StreamWriter(
                context.TestRunDirectory + @"\checkLog.txt", false, System.Text.Encoding.UTF8);
            Context.AddResultFile("checkLog.txt");
            
            //Blink用のテストデータ生成
            System.IO.Directory.CreateDirectory(@".\TestDatas");
            System.IO.File.Delete(@"TestDatas\blinkCookies.sqlite3");
            var dbClient = new SQLiteConnection(@"Data Source=TestDatas\blinkCookies.sqlite3");
            try
            {
                dbClient.Open();
                var query = dbClient.CreateCommand();
                query.CommandText = SQLITE_QUERY_CREATE_TABLE_COOKIES;
                query.ExecuteNonQuery();

                query = dbClient.CreateCommand();
                query.CommandText = SQLITE_QUERY_CREATE_TABLE_META;
                query.ExecuteNonQuery();

                foreach (var item in SqliteInsertQueries.Select((item, idx) => new { CommandText = item, Index = idx }))
                {
                    var param = new SQLiteParameter("@encryptData", System.Data.DbType.Binary)
                    {
                        Value = Win32Api.CryptProtectedData(
                            System.Text.Encoding.UTF8.GetBytes(string.Format("binary_data{0:00000}", (item.Index + 1) * 10000 + 10))),
                    };
                    query = dbClient.CreateCommand();
                    query.CommandText = string.Format(item.CommandText,
                        (Utility.DateTimeToUnixTime(DateTime.UtcNow.AddYears(1)) + 11644473600) * 1000000);
                    query.Parameters.Add(param);
                    query.ExecuteNonQuery();
                }
            }
            finally
            { dbClient.Close(); }
        }
        [ClassCleanup]
        public static void Cleanup()
        {
            LogWriter.Flush();
            LogWriter.Close();
            LogWriter.Dispose();
        }

        [TestMethod]
        public async Task CheckTest()
        {
            LogWriter.WriteLine("===================");
            LogWriter.WriteLine(" Check Test");
            LogWriter.WriteLine("===================");

            //使用不可なImporterの確認
            foreach (var item in (await CookieGetters.Default.GetInstancesAsync(false)).Where(importer => !importer.IsAvailable))
                await CheckImporters(item, CookieImportState.Unavailable, false, false);

            //使用可能なImporterの確認
            foreach (var item in (await CookieGetters.Default.GetInstancesAsync(true)))
                await CheckImporters(item, CookieImportState.Success, true, false);

            LogWriter.WriteLine();
        }
        [TestMethod]
        public async Task RegressionTest()
        {
            LogWriter.WriteLine("===================");
            LogWriter.WriteLine(" Regression Test");
            LogWriter.WriteLine("===================");
            foreach (var filePath in System.IO.Directory.EnumerateFiles(REGRESSION_TESTDATA_PATH, "*.blink"))
            {
                var browserName = System.IO.Path.GetFileNameWithoutExtension(filePath);
                var importer = new BlinkCookieImporter(new CookieSourceInfo(browserName, "Default", filePath, typeof(BlinkImporterFactory).FullName, false), 2);
                await CheckImporters(importer, CookieImportState.Success, true, true);
            }
            LogWriter.WriteLine();
        }
        [TestMethod]
        public async Task BlinkImporterTest()
        {
            LogWriter.WriteLine("===================");
            LogWriter.WriteLine(" BlinkCookieImporter Test");
            LogWriter.WriteLine("===================");
            
            //Cookiesが存在する場合
            var importer = new BlinkCookieImporter(
                new CookieSourceInfo("BlinkBrowser_blinkCookies.sqlite3", "BlinkProfile", @".\TestDatas\blinkCookies.sqlite3", null, false), 2);
            await CheckImporters(importer, CookieImportState.Success, true, true);

            //Cookiesが存在しない場合
            importer = new BlinkCookieImporter(
                new CookieSourceInfo("BlinkBrowser_empty", "BlinkProfile", string.Empty, null, false), 2);
            await CheckImporters(importer, CookieImportState.Unavailable, false, false);

            LogWriter.WriteLine();
        }
        [TestMethod]
        public void IEPMImporterProxyTest()
        {
            var cookieHeader = IEPMCookieImporter.InternalGetCookiesWinApiOnProxy(new Uri("http://nicovideo.jp"), null);
            Assert.AreNotEqual(null, cookieHeader);
            Assert.AreNotEqual(string.Empty, cookieHeader);
        }
        [TestMethod]
        public async Task RxTest()
        {
            var importResult = await CookieGetters.Default.GetInstancesAsync(true).ToObservable()
                .SelectMany(items => items)
                .SelectMany(item => item.GetCookiesAsync(new Uri("http://nicovideo.jp")))
                .SelectMany(async item => new { ImportedObj = item, IsLogined = (await GetUserName(item)) != null })
                .Where(item => item.IsLogined)
                .Select(item => item.ImportedObj);
        }

        static async Task CheckImporters(ICookieImporter importer, CookieImportState expectedState, bool expectedIsAvailable, bool checkUserSession)
        {
            var cookies = new CookieContainer();
            var url = new Uri("http://nicovideo.jp/");
            var res = await importer.GetCookiesAsync(url);
            res.AddTo(cookies);

            LogWriter.WriteLine("{0}:\t{1} ({2})",
                importer.IsAvailable ? "OK" : "Error", importer.SourceInfo.BrowserName, importer.SourceInfo.ProfileName);

            Assert.AreEqual(expectedState, res.Status);
            Assert.IsNotNull(importer.SourceInfo.BrowserName);
            Assert.IsNotNull(importer.SourceInfo.ProfileName);
            Assert.AreEqual(expectedIsAvailable, importer.IsAvailable);

            if (expectedIsAvailable)
            {
                Assert.IsNotNull(importer.SourceInfo.CookiePath,
                    string.Format("{0}のSourceInfo.CookiePathにはプロフィール名が入りますが、取得できませんでした。", importer.SourceInfo.BrowserName));
                Assert.AreNotEqual(0, cookies.GetCookies(url).Count,
                    string.Format("{0}からCookieを取得できませんでした。", importer.SourceInfo.BrowserName));

                if (checkUserSession)
                    Assert.IsNotNull(cookies.GetCookies(new Uri("http://nicovideo.jp"))["user_session"]);
            }
            else
            {
                Assert.AreEqual(0, cookies.GetCookies(url).Count,
                    string.Format("{0}からIsAvailableがfalse状態でCookieを取得する事は出来ませんが、取得されてしまっています。", importer.SourceInfo.BrowserName));
            }
        }
        static async Task<string> GetUserName(CookieImportResult importObj)
        {
            try
            {
                var url = new Uri("http://www.nicovideo.jp/my/channel");
                var container = new CookieContainer();
                var client = new HttpClient(new HttpClientHandler() { CookieContainer = container });
                if (importObj.AddTo(container) != CookieImportState.Success)
                    return null;

                var res = await client.GetStringAsync(url);
                if (string.IsNullOrEmpty(res))
                    return null;
                var namem = Regex.Match(res, "nickname = \"([^<>]+)\";", RegexOptions.Singleline);
                if (namem.Success)
                    return namem.Groups[1].Value;
                else
                    return null;
            }
            catch (System.Net.Http.HttpRequestException) { return null; }
        }
    }
}
