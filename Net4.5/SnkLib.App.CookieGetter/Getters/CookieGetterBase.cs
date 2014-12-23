using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SunokoLibrary.Application.Browsers
{
    /// <summary>
    /// ICookieImporterの実装の作成を支援する基盤クラスです。
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{Config.BrowserName,nq}({Config.ProfileName,nq}): {Config.CookiePath,nq}")]
    public abstract class CookieGetterBase : ICookieImporter
    {
        /// <summary>
        /// 指定されたconfigからCookieを取得するICookieImporterを生成します。
        /// 継承時にコンストラクタでcookiePathType, primaryLevelを直接指定し、
        /// configの方は引数で受け取る形にする事を推奨します。
        /// </summary>
        /// <param name="config">対象のブラウザの構成情報</param>
        /// <param name="cookiePathType">対象ブラウザのCookieの置き場所の形式</param>
        /// <param name="primaryLevel">
        /// 並べ替え時に用いられる数値。
        /// OSブラウザ: 0、有名ブラウザ: 1、派生ブラウザ: 2
        /// </param>
        public CookieGetterBase(BrowserConfig config, PathType cookiePathType, int primaryLevel)
        {
            if (config == null)
                throw new ArgumentNullException("引数statusがnullです。");
            Config = config;
            CookiePathType = cookiePathType;
            PrimaryLevel = primaryLevel;
        }

#pragma warning disable 1591

        public BrowserConfig Config { get; private set; }
        public PathType CookiePathType { get; private set; }
        public int PrimaryLevel { get; private set; }
        public virtual bool IsAvailable
        {
            get
            {
                return string.IsNullOrEmpty(Config.CookiePath)
                    ? false : System.IO.File.Exists(Config.CookiePath);
            }
        }
        public Task<ImportResult> GetCookiesAsync(Uri targetUrl, CookieContainer container)
        {
            //同期コンテキストが確実にUIスレッド以外になるようにする。
            //awaitのある処理をUIスレッドでTask.Wait()した場合、デッドロックが発生する事がある。
            //その対策としてここで別スレッドから呼び出す事を保証する。
            return Task.Factory.StartNew(() => ProtectedGetCookies(targetUrl, container));
        }
        public abstract ICookieImporter Generate(BrowserConfig config);

#pragma warning restore 1591

        /// <summary>
        /// Cookie取得処理の本体。
        /// </summary>
        /// <param name="targetUrl">Cookieが送信されるURL</param>
        /// <param name="container">取得結果が追加されるコンテナ</param>
        /// <returns>処理結果の状態</returns>
        protected abstract ImportResult ProtectedGetCookies(Uri targetUrl, CookieContainer container);
        /// <summary>
        /// 失敗した処理の情報を出力します。
        /// </summary>
        /// <param name="target">失敗した処理が行われた対象</param>
        /// <param name="message">失敗した処理の名前</param>
        /// <param name="detailMessage">詳細な状況説明文</param>
        protected static void TraceFail(ICookieImporter target, string message, string detailMessage)
        { Trace.Fail(string.Format("{0}のCookieの{1}", target.Config.BrowserName, message), detailMessage); }
    }
}