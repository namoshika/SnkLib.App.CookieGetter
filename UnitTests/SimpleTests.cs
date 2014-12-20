using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Net;
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

            //各ブラウザ向けGetterの動作確認結果を一覧として出力させる。
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

            //使用不可なGetterの確認
            foreach (var item in (await CookieGetters.Default.GetInstancesAsync(false)).Where(getter => !getter.IsAvailable))
                await CheckGetters(item, false, false);

            //使用可能なGetterの確認
            foreach (var item in (await CookieGetters.Default.GetInstancesAsync(true)))
                await CheckGetters(item, true, false);

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
                var getter = new BlinkCookieGetter(new BrowserConfig(browserName, "Default", filePath, BlinkBrowserManager.ENGINE_ID, false), 2);
                await CheckGetters(getter, true, true);
            }
            LogWriter.WriteLine();
        }
        [TestMethod]
        public async Task BlinkGetterTest()
        {
            LogWriter.WriteLine("===================");
            LogWriter.WriteLine(" BlinkGetter Test");
            LogWriter.WriteLine("===================");
            var getter = new BlinkCookieGetter(
                new BrowserConfig("BlinkBrowser", "BlinkProfile", @".\TestDatas\blinkCookies.sqlite3", BlinkBrowserManager.ENGINE_ID, false), 2);
            await CheckGetters(getter, true, true);
            LogWriter.WriteLine();
        }

        async Task CheckGetters(ICookieImporter getter, bool expectedIsAvailable, bool checkUserSession)
        {
            var cookies = new CookieContainer();
            var url = new Uri("http://nicovideo.jp/");
            await getter.GetCookiesAsync(url, cookies);

            LogWriter.WriteLine("{0}:\t{1} ({2})",
                getter.IsAvailable ? "OK" : "Error", getter.Config.BrowserName, getter.Config.ProfileName);

            Assert.IsNotNull(getter.Config.BrowserName,
                string.Format("Config.BrowserNameにはブラウザ名が入りますが、取得できませんでした。"));
            Assert.IsNotNull(getter.Config.ProfileName,
                string.Format("{0}のConfig.ProfileNameにはプロフィール名が入りますが、取得できませんでした。", getter.Config.BrowserName));

            if (expectedIsAvailable)
            {
                Assert.IsTrue(getter.IsAvailable,
                    string.Format("{0}のIsAvailableはtrueである必要がありますが、false状態です。", getter.Config.BrowserName));
                Assert.IsNotNull(getter.Config.CookiePath,
                    string.Format("{0}のConfig.CookiePathにはプロフィール名が入りますが、取得できませんでした。", getter.Config.BrowserName));
                Assert.AreNotEqual(0, cookies.GetCookies(url).Count,
                    string.Format("{0}からCookieを取得できませんでした。", getter.Config.BrowserName));

                if (checkUserSession)
                    Assert.IsNotNull(cookies.GetCookies(new Uri("http://nicovideo.jp"))["user_session"]);
            }
            else
            {
                Assert.IsFalse(getter.IsAvailable,
                    string.Format("{0}のIsAvailableはfalseである必要がありますが、true状態です。", getter.Config.BrowserName));
                Assert.AreEqual(0, cookies.GetCookies(url).Count,
                    string.Format("{0}からIsAvailableがfalse状態でCookieを取得する事は出来ませんが、取得されてしまっています。", getter.Config.BrowserName));
            }
        }
    }
}
