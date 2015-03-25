using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace SunokoLibrary.Application.Browsers
{
    /// <summary>
    /// IEからCookieを取得します。
    /// </summary>
    public class IECookieImporter : CookieImporterBase
    {
#pragma warning disable 1591

        public IECookieImporter(CookieSourceInfo info, int primaryLevel) : base(info, CookiePathType.Directory, primaryLevel) { }
        public override bool IsAvailable { get { return true; } }
        public override ICookieImporter Generate(CookieSourceInfo newInfo)
        { return new IECookieImporter(newInfo, PrimaryLevel); }
        protected override CookieImportResult ProtectedGetCookies(Uri targetUrl)
        {
            string cookiesText;
            var hResult = Win32Api.GetCookiesFromIE(out cookiesText, targetUrl, null);
            if (cookiesText == null)
                return new CookieImportResult(null, CookieImportState.AccessError);
            try
            {
                var cookies = new CookieCollection();
                foreach (var item in ParseCookies(cookiesText, targetUrl))
                    cookies.Add(item);
                return new CookieImportResult(cookies, CookieImportState.Success);
            }
            catch (CookieImportException ex)
            {
                TraceError(this, "Cookie読み込みに失敗。", ex.ToString());
                return new CookieImportResult(null, ex.Result);
            }
        }

        //渡されたcookieヘッダーをcookieに変換する。
        //CookieContainer.SetCookies()は不都合が生じる。
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
                    TraceError(this, "IE Cookie解析に失敗。", string.Format("cookieHeader: {0}\r\nurl: {1}", cookieHeader, url));
                    throw new CookieImportException("IE Cookieの解析に失敗。", CookieImportState.ConvertError);
                }
                var name = chunks[0].Trim();
                var value = chunks[1].Trim();
                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(value))
                {
                    TraceError(this, "IE Cookieの解析に失敗。", string.Format("cookieHeader: {0}\r\nurl: {1}", cookieHeader, url));
                    throw new CookieImportException("IE Cookieの解析に失敗。", CookieImportState.ConvertError);
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

#pragma warning restore 1591
    }
}