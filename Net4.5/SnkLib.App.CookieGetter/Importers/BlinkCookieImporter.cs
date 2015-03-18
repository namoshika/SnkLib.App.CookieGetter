using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace SunokoLibrary.Application.Browsers
{
    /// <summary>
    /// GoogleChromeからCookieを取得します。
    /// </summary>
    public class BlinkCookieImporter : SqlCookieImporter
    {
#pragma warning disable 1591

        public BlinkCookieImporter(CookieSourceInfo info, int primaryLevel) : base(info, primaryLevel) { }
        const string SELECT_QUERY_VERSION = "SELECT value FROM meta WHERE key='version';";
        const string SELECT_QUERY = "SELECT value, name, host_key, path, expires_utc FROM cookies";
        const string SELECT_QUERY_V7 = "SELECT encrypted_value, name, host_key, path, expires_utc FROM cookies";

        public override ICookieImporter Generate(CookieSourceInfo newInfo)
        { return new BlinkCookieImporter(newInfo, PrimaryLevel); }
        protected override CookieImportResult ProtectedGetCookies(Uri targetUrl)
        {
            if (IsAvailable == false)
                return new CookieImportResult(null,CookieImportState.Unavailable);
            try
            {
                var formatVersionRec = LookupEntry(SourceInfo.CookiePath, SELECT_QUERY_VERSION);
                int formatVersion;
                if (formatVersionRec.Count == 0
                    || formatVersionRec[0].Length == 0
                    || int.TryParse((string)formatVersionRec[0][0], out formatVersion) == false)
                    return new CookieImportResult(null,CookieImportState.ConvertError);

                string query;
                query = formatVersion < 7 ? SELECT_QUERY : SELECT_QUERY_V7;
                query = string.Format("{0} {1} ORDER BY creation_utc DESC", query, MakeWhere(targetUrl));
                var cookies = new CookieCollection();
                foreach (var item in LookupCookies(SourceInfo.CookiePath, query, rec => DataToCookie(rec, formatVersion)))
                    cookies.Add(item);
                return new CookieImportResult(cookies, CookieImportState.Success);
            }
            catch (CookieImportException ex)
            {
                TraceFail(this, "取得に失敗しました。", ex.ToString());
                return new CookieImportResult(null, ex.Result);
            }
        }
        protected Cookie DataToCookie(object[] data, int formatVersion)
        {
            if (formatVersion < 7
                ? data.Take(4).Where(rec => rec is string == false).Any() || data[4] is long == false
                : data[0] is byte[] == false || data.Skip(1).Take(3).Where(rec => rec is string == false).Any() || data[4] is long == false)
                throw new CookieImportException(
                    "未知の項目をレコードから発見。レコードからCookieオブジェクトへの変換に失敗しました。", CookieImportState.ConvertError);

            var expiresDt = (ulong)(long)data[4];
            var baseObj = new Cookie()
            {
                Name = data[1] as string,
                Domain = data[2] as string,
                Path = data[3] as string,
                Expires = expiresDt > 0
                    ? Utility.UnixTimeToDateTime(expiresDt / 1000000 - 11644473600) : DateTime.MinValue,
            };

            //Cookieの値の読み込み。値はURLエンコード済みなのでそのまま放り込む。
            //列数6ならばCookie格納方法のバージョンはCookieが暗号化された7以降と分かる
            if (formatVersion >= 7)
            {
                var cipher = data[0] as byte[];
                if (cipher == null || cipher.Length == 0)
                    throw new CookieImportException(
                        "Cookieファイルから暗号化データを取得できませんでした。", CookieImportState.ConvertError);
                var plain = Win32Api.DecryptProtectedData(cipher);
                if (plain == null)
                    throw new CookieImportException(
                        "Cookieの暗号化データを復号化できませんでした。", CookieImportState.ConvertError);
                baseObj.Value = Encoding.UTF8.GetString(plain);
            }
            else
                baseObj.Value = data[0] as string;
            return baseObj;
        }
        protected string MakeWhere(Uri url)
        {
            //A.B.comを[[com], [B, com], [A, B, com]]な形にする
            //メインドメインまでのサブドメインの全パターンを持った配列を作る
            var domains = url.Host.Split('.')
                .Reverse().Aggregate(
                    Enumerable.Repeat(Enumerable.Empty<string>(), 1),
                    (tmp, val) => tmp.Concat(Enumerable.Repeat(Enumerable.Repeat(val, 1).Concat(tmp.Last()), 1)))
                .Skip(2)
                .Select(levels => string.Join(".", levels.ToArray()))
                .SelectMany(domain => new[] { domain, "." + domain });
            //全てのドメインをOR文で結ぶ
            var query = string.Format(" WHERE ({0})", string.Join(
                " OR ", domains.Select(domain => string.Format("host_key = \"{0}\"", domain)).ToArray()));
            return query;
        }

#pragma warning restore 1591
    }
}
