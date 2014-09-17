using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SunokoLibrary.Application.Browsers
{
    public class WebkitQtCookieGetter : CookieGetterBase
    {
        public WebkitQtCookieGetter(BrowserConfig config) : base(config, PathType.File) { }
        public override ICookieImporter Generate(BrowserConfig config)
        { return new WebkitQtCookieGetter(config); }
        protected override async Task<ImportResult> ProtectedGetCookiesAsync(Uri targetUrl, CookieContainer container)
        {
            if (IsAvailable == false)
                return ImportResult.Unavailable;
            try
            {
                var res = ImportResult.ConvertError;
                using (var sr = new System.IO.StreamReader(Config.CookiePath))
                    while (!sr.EndOfStream)
                    {
                        var line = await sr.ReadLineAsync();
                        if (line.StartsWith("cookies="))
                            container.Add(ParseCookieSettings(line));
                        res = ImportResult.Success;
                    }
                return res;
            }
            catch (System.IO.IOException ex)
            {
                TraceFail(this, "読み込みでエラーが発生しました。", ex.ToString());
                return ImportResult.AccessError;
            }
            catch (Exception ex)
            {
                TraceFail(this, "読み込みでエラーが発生しました。", ex.ToString());
                return ImportResult.ConvertError;
            }
        }

        CookieCollection ParseCookieSettings(string line)
        {
            var container = new CookieCollection();

            // クッキー情報の前についているよくわからないヘッダー情報を取り除く
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
