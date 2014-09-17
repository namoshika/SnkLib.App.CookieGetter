using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SunokoLibrary.Application.Browsers
{
    /// <summary>
    /// GoogleChromeからクッキーを取得する
    /// </summary>
    public class BlinkCookieGetter : SqlCookieGetter
    {
        public BlinkCookieGetter(BrowserConfig config) : base(config) { }
        const string SELECT_QUERY_VERSION = "SELECT value FROM meta WHERE key='version';";
        const string SELECT_QUERY = "SELECT 0, value, name, host_key, path, expires_utc FROM cookies";
        const string SELECT_QUERY_V7 = "SELECT 7, encrypted_value, name, host_key, path, expires_utc FROM cookies";

        public override ICookieImporter Generate(BrowserConfig config)
        { return new BlinkCookieGetter(config); }
        protected override async Task<ImportResult> ProtectedGetCookiesAsync(Uri targetUrl, CookieContainer container)
        {
            if (IsAvailable == false)
                return ImportResult.Unavailable;
            try
            {
                var formatVersionRec = await LookupEntryAsync(Config.CookiePath, SELECT_QUERY_VERSION);
                int cookieFormatVersion;
                if (formatVersionRec.Count == 0
                    || formatVersionRec[0].Length == 0
                    || int.TryParse((string)formatVersionRec[0][0], out cookieFormatVersion) == false)
                    return ImportResult.ConvertError;

                string query;
                query = cookieFormatVersion < 7 ? SELECT_QUERY : SELECT_QUERY_V7;
                query = string.Format("{0} {1} ORDER BY creation_utc DESC", query, MakeWhere(targetUrl));
                container.Add(await LookupCookiesAsync(Config.CookiePath, query));
                return ImportResult.Success;
            }
            catch (CookieImportException ex)
            {
                TraceFail(this, "取得に失敗しました。", ex.ToString());
                return ex.Result;
            }
        }
        protected override Cookie DataToCookie(object[] data)
        {
            long formatVersion;
            if(data.Length < 6 || data[0] is long == false)
                throw new CookieImportException(
                    "CookieFormatVersionの取得に失敗。レコードからCookieオブジェクトへの変換に失敗しました。", ImportResult.ConvertError);
            formatVersion = (long)data[0];
            if(formatVersion < 7
                ? data.Skip(1).Take(4).Where(rec => rec is string == false).Any() || data[5] is long == false
                : data[1] is byte[] == false || data.Skip(2).Take(3).Where(rec => rec is string == false).Any() || data[5] is long == false)
                throw new CookieImportException(
                    "未知の項目をレコードから発見。レコードからCookieオブジェクトへの変換に失敗しました。", ImportResult.ConvertError);

            var expiresDt = (long)data[5];
            Cookie baseObj = new Cookie()
            {
                Name = data[2] as string,
                Domain = data[3] as string,
                Path = data[4] as string,
                Expires = Utility.UnixTimeToDateTime((int)(expiresDt / 1000000 - 11644473600))
            };

            //Cookieの値の読み込み
            //列数6ならばCookie格納方法のバージョンはCookieが暗号化された7以降と分かる
            if (formatVersion >= 7)
            {
                var cipher = data[1] as byte[];
                if (cipher == null || cipher.Length == 0)
                    throw new CookieImportException(ImportResult.ConvertError);
                else
                {
                    //リソース確保
                    Win32Api.DATA_BLOB input;
                    input.pbData = Marshal.AllocHGlobal(cipher.Length);
                    input.cbData = (uint)cipher.Length;
                    Marshal.Copy(cipher, 0, input.pbData, cipher.Length);
                    var output = new Win32Api.DATA_BLOB();
                    var dammy = new Win32Api.DATA_BLOB();

                    //復号化
                    bool isSucc;
                    isSucc = Win32Api.CryptUnprotectData(ref input, null, ref dammy, IntPtr.Zero, IntPtr.Zero, 0, ref output);
                    Debug.Assert(isSucc, "CryptUnprotectData error: BlinkのCookie復号に失敗。");
                    if (isSucc == false)
                        throw new CookieImportException(ImportResult.ConvertError);

                    //リソース解放
                    var plain = new byte[output.cbData];
                    Marshal.Copy(output.pbData, plain, 0, (int)output.cbData);
                    baseObj.Value = Encoding.UTF8.GetString(plain);
                    //output解放
                    isSucc = Win32Api.LocalFree(output.pbData) == IntPtr.Zero;
                    Debug.Assert(isSucc, "CryptUnprotectData error: BlinkのCookie復号後のoutputリソース解放に失敗。");
                    //input解放
                    isSucc = Win32Api.LocalFree(input.pbData) == IntPtr.Zero;
                    Debug.Assert(isSucc, "CryptUnprotectData error: BlinkのCookie復号後のinputリソース解放に失敗。");
                }
            }
            else
                baseObj.Value = (data[1] as string).Replace(";", "%3b").Replace(",", "%2c");
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
                .Select(levels => string.Join(".", levels))
                .SelectMany(domain => new[] { domain, "." + domain });
            //全てのドメインをOR文で結ぶ
            var query = string.Format(" WHERE ({0})", string.Join(
                " OR ", domains.Select(domain => string.Format("host_key = \"{0}\"", domain))));
            return query;
        }
    }
}
