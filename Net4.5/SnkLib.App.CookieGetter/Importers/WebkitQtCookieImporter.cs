using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
#if !NET20
using System.Threading.Tasks;
#endif

namespace SunokoLibrary.Application.Browsers
{
    /// <summary>
    /// Qt上のWebkitでよく使われるCookieファイル形式からCookieを取得します。
    /// </summary>
    public class WebkitQtCookieImporter : CookieImporterBase
    {
#pragma warning disable 1591
        public WebkitQtCookieImporter(CookieSourceInfo info, int primaryLevel) : base(info, CookiePathType.File, primaryLevel) { }
        public override ICookieImporter Generate(CookieSourceInfo newInfo)
        { return new WebkitQtCookieImporter(newInfo, PrimaryLevel); }
        protected override CookieImportResult ProtectedGetCookies(Uri targetUrl)
        {
            if (IsAvailable == false)
                return new CookieImportResult(null, CookieImportState.Unavailable);
            try
            {
                var cookies = new CookieCollection();
                var res = CookieImportState.ConvertError;
                using (var sr = new System.IO.StreamReader(SourceInfo.CookiePath))
                    while (!sr.EndOfStream)
                    {
                        var line = sr.ReadLine();
                        if (line.StartsWith("cookies="))
                            cookies.Add(ParseCookieSettings(line));
                        res = CookieImportState.Success;
                    }
                return new CookieImportResult(cookies, res);
            }
            catch (System.IO.IOException ex)
            {
                TraceError(this, "読み込みでエラーが発生しました。", ex.ToString());
                return new CookieImportResult(null,CookieImportState.AccessError);
            }
            catch (Exception ex)
            {
                TraceError(this, "読み込みでエラーが発生しました。", ex.ToString());
                return new CookieImportResult(null, CookieImportState.ConvertError);
            }
        }
#pragma warning restore 1591

        CookieCollection ParseCookieSettings(string line)
        {
            var container = new CookieCollection();

            // Cookie情報の前についているよくわからないヘッダー情報を取り除く
            // 対象：
            // 　\\xと２桁の１６進数値
            // 　\\\\
            // 　\がない場合の先頭１文字
            var matchPattern = "^(\\\\x[0-9a-fA-F]{2})|^(\\\\\\\\)|^(.)|[\"()]";
            var reg = new System.Text.RegularExpressions.Regex(matchPattern, System.Text.RegularExpressions.RegexOptions.Compiled);
            var blocks = line.Split(new string[] { "\\0\\0\\0" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var block in blocks)
                if (block.Contains("=") && block.Contains("domain"))
                {
                    var header = reg.Replace(block, "");
                    var cookie = ParseCookie(header);
                    if (cookie != null)
                        container.Add(cookie);
                }

            return container;
        }
        Cookie ParseCookie(string header)
        {
            if (string.IsNullOrEmpty(header))
                throw new ArgumentException("header");

            var cookie = new System.Net.Cookie();
            var isCookieHeader = false;

            foreach (var segment in header.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
            {
                var kvp = ParseKeyValuePair(segment.Trim());
                if (string.IsNullOrEmpty(kvp.Key))
                {
                    isCookieHeader = false;
                    break;
                }
                switch (kvp.Key)
                {
                    case "domain":
                        cookie.Domain = kvp.Value;
                        isCookieHeader = true;
                        break;
                    case "expires":
                        cookie.Expires = DateTime.Parse(kvp.Value);
                        break;
                    case "path":
                        cookie.Path = kvp.Value;
                        break;
                    default:
                        cookie.Name = kvp.Key;
                        cookie.Value = kvp.Value;
                        if (cookie.Value != null)
                        {
                            cookie.Value = Uri.EscapeDataString(cookie.Value);
                        }
                        break;
                }
            }
            return isCookieHeader ? cookie : null;
        }
        KeyValuePair<string, string> ParseKeyValuePair(string exp)
        {
            var eqindex = exp.IndexOf('=');
            return eqindex != -1
                ? new KeyValuePair<string, string>(exp.Substring(0, eqindex), exp.Substring(eqindex + 1))
                : new KeyValuePair<string, string>();
        }
    }
}
