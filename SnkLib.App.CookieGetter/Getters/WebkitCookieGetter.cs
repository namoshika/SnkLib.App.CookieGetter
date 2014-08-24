using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System.Text;
using System.Data.SQLite;

namespace SunokoLibrary.Application.Browsers
{
    /// <summary>
    /// Webkit系のブラウザからクッキーを取得するクラス
    /// </summary>
    public abstract class WebkitCookieGetter : SqlCookieGetter
    {
        public WebkitCookieGetter(BrowserConfig option) : base(option) { }
        const string SELECT_QUERY_VERSION = "SELECT value FROM meta WHERE key='version';";
        const string SELECT_QUERY_V7 = "SELECT value, name, host_key, path, expires_utc, encrypted_value FROM cookies";
        const int VERSION = 7;

        public override bool GetCookies(Uri targetUrl, CookieContainer container)
        {
            if (IsAvailable == false)
                return false;
            try
            {
                var cookieFormatVersion = int.Parse((string)LookupEntry(Config.CookiePath, SELECT_QUERY_VERSION)[0][0]);
                var query = MakeQuery(targetUrl);
                if (cookieFormatVersion >= VERSION)
                    query = query.Replace("expires_utc FROM cookies", "expires_utc, encrypted_value FROM cookies");
                container.Add(LookupCookies(Config.CookiePath, query));
                return true;
            }
            catch { return false; }
        }
        protected override string MakeQuery(Uri url)
        { return string.Format("{0} {1} ORDER BY creation_utc DESC", SELECT_QUERY_V7, MakeWhere(url)); }
        protected string MakeWhere(Uri url)
        {
            //A.B.comを[[com], [B, com], [A, B, com]]な形にする
            //メインドメインまでのサブドメインの全パターンを持った配列を作る
            var domains = url.Host.Split('.')
                .Reverse().Aggregate(
                    Enumerable.Repeat(Enumerable.Empty<string>(), 1),
                    (tmp, val) => tmp.Concat(Enumerable.Repeat(Enumerable.Repeat(val, 1).Concat(tmp.Last()), 1)))
                .Skip(2)
                .Select(levels => string.Join(".", levels))
                .SelectMany(domain => new[] { domain, "." + domain });
            //全てのドメインをOR文で結ぶ
            var query = string.Format(" WHERE ({0})", string.Join(
                " OR ", domains.Select(domain => string.Format("host_key = \"{0}\"", domain))));
            return query;
        }
    }
}