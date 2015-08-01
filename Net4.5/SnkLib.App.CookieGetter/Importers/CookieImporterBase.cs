using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
#if !NET20
using System.Threading.Tasks;
#endif

namespace SunokoLibrary.Application.Browsers
{
    /// <summary>
    /// ICookieImporterの実装の作成を支援する基盤クラスです。
    /// </summary>
    [DebuggerDisplay("{SourceInfo.BrowserName,nq}({SourceInfo.ProfileName,nq}): {SourceInfo.CookiePath,nq}")]
    public abstract class CookieImporterBase : ICookieImporter
    {
        /// <summary>
        /// 指定されたinfoからCookieを取得するICookieImporterを生成します。
        /// 継承時にコンストラクタでcookiePathType, primaryLevelを直接指定し、
        /// infoの方は引数で受け取る形にする事を推奨します。
        /// </summary>
        /// <param name="info">対象のブラウザの構成情報</param>
        /// <param name="cookiePathType">対象ブラウザのCookieの置き場所の形式</param>
        /// <param name="primaryLevel">
        /// 並べ替え時に用いられる数値。
        /// OSブラウザ: 0、有名ブラウザ: 1、派生ブラウザ: 2
        /// </param>
        public CookieImporterBase(CookieSourceInfo info, CookiePathType cookiePathType, int primaryLevel)
        {
            if (info == null)
                throw new ArgumentNullException("引数infoがnullです。");
            SourceInfo = info;
            CookiePathType = cookiePathType;
            PrimaryLevel = primaryLevel;
        }

#pragma warning disable 1591
        public CookieSourceInfo SourceInfo { get; private set; }
        public CookiePathType CookiePathType { get; private set; }
        public int PrimaryLevel { get; private set; }
        public virtual bool IsAvailable
        {
            get
            {
                return string.IsNullOrEmpty(SourceInfo.CookiePath)
                    ? false : System.IO.File.Exists(SourceInfo.CookiePath);
            }
        }
        public abstract ICookieImporter Generate(CookieSourceInfo newInfo);
        public Task<CookieImportResult> GetCookiesAsync(Uri targetUrl)
        {
            //同期コンテキストが確実にUIスレッド以外になるようにする。
            //awaitのある処理をUIスレッドでTask.Wait()した場合、デッドロックが発生する事がある。
            //その対策としてここで別スレッドから呼び出す事を保証する。
            return Task.Factory.StartNew(() => ProtectedGetCookies(targetUrl));
        }
#pragma warning restore 1591

        /// <summary>
        /// Cookie取得処理の本体。
        /// </summary>
        /// <param name="targetUrl">Cookieが送信されるURL</param>
        /// <returns>処理結果の状態</returns>
        protected abstract CookieImportResult ProtectedGetCookies(Uri targetUrl);
        /// <summary>
        /// 失敗した処理の情報を出力します。
        /// </summary>
        /// <param name="target">失敗した処理が行われた対象</param>
        /// <param name="message">失敗した処理の名前</param>
        /// <param name="detailMessage">詳細な状況説明文</param>
        protected static void TraceError(ICookieImporter target, string message, string detailMessage)
        { Trace.TraceError(string.Format("SnkLib.App.CookieGetter.dll:\r\n{0}のCookieの{1}", target.SourceInfo.BrowserName, message), detailMessage); }
    }
}