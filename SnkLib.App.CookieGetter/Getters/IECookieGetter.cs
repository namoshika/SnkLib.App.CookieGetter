using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SunokoLibrary.Application.Browsers
{
    /// <summary>
    /// IEやトライデントエンジンを利用しているブラウザのクッキーを取得する
    /// </summary>
    public class IECookieGetter : CookieGetterBase
    {
        public IECookieGetter(BrowserConfig config) : base(config, PathType.Directory) { }
        public override bool IsAvailable { get { return true; } }
        public override ICookieImporter Generate(BrowserConfig config)
        { return new IECookieGetter(config); }
        protected override async Task<ImportResult> ProtectedGetCookiesAsync(Uri targetUrl, System.Net.CookieContainer container)
        {
            string cookiesText;
            var hResult = Win32Api.GetCookiesFromIE(out cookiesText, targetUrl, null);
            Debug.Assert(cookiesText != null, "InternetGetCookie error code: " + hResult);

            if (cookiesText == null)
                return ImportResult.AccessError;
            try
            {
                await Task.Yield();
                var cookies = new CookieCollection();
                foreach (var item in ParseCookies(cookiesText, targetUrl))
                    cookies.Add(item);
                container.Add(cookies);
                return ImportResult.Success;
            }
            catch (CookieImportException ex)
            {
                TraceFail(this, "Cookie読み込みに失敗。", ex.ToString());
                return ex.Result;
            }
        }

        /// <summary>
        /// 渡されたcookieヘッダーをcookieに変換する。CookieContainer.SetCookies()は不都合が生じる。
        /// </summary>
        /// <exception cref="CookieImportException" />
        protected virtual IEnumerable<Cookie> ParseCookies(string cookieHeader, Uri url)
        {
            if (string.IsNullOrEmpty(cookieHeader))
                yield break;

            var cookiesText = cookieHeader.Split(';');
            foreach (var data in cookiesText)
            {
                var cookie = new Cookie();
                var chunks = data.ToString().Split('=');
                if (2 > chunks.Length)
                {
                    TraceFail(this, "IEクッキーの解析に失敗。", string.Format("cookieHeader: {0}\r\nurl: {1}", cookieHeader, url));
                    throw new CookieImportException("IEクッキーの解析に失敗。", ImportResult.ConvertError);
                }
                var name = chunks[0].Trim();
                var value = chunks[1].Trim();
                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(value))
                {
                    TraceFail(this, "IEクッキーの解析に失敗。", string.Format("cookieHeader: {0}\r\nurl: {1}", cookieHeader, url));
                    throw new CookieImportException("IEクッキーの解析に失敗。", ImportResult.ConvertError);
                }

                cookie.Name = name;
                cookie.Value = value;
                cookie.Domain = url.Host;
                //cookie.Path = url.AbsolutePath;
                //このほうがいいきがする 2011-11-19
                cookie.Path = url.Segments[0];
                //有効期限適当付与 2013-07-03
                cookie.Expires = DateTime.Now.AddDays(30);
                yield return cookie;
            }
        }
    }
}