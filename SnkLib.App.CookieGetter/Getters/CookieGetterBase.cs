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
    /// 指定したブラウザからクッキーを取得する
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{Config.BrowserName,nq}({Config.ProfileName,nq}): {Config.CookiePath,nq}")]
    public abstract class CookieGetterBase : ICookieImporter
    {
        public CookieGetterBase(BrowserConfig config, PathType cookiePathType)
        {
            if (config == null)
                throw new ArgumentNullException("引数statusがnullです。");
            Config = config;
            CookiePathType = cookiePathType;
        }

        public BrowserConfig Config { get; private set; }
        public PathType CookiePathType { get; private set; }
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
            return Task.Run(() => ProtectedGetCookiesAsync(targetUrl, container));
        }
        public abstract ICookieImporter Generate(BrowserConfig config);

        protected abstract Task<ImportResult> ProtectedGetCookiesAsync(Uri targetUrl, CookieContainer container);
        protected static void TraceFail(ICookieImporter target, string message, string detailMessage)
        { Trace.Fail(string.Format("{0}のCookieの{1}", target.Config.BrowserName, message), detailMessage); }
    }
}