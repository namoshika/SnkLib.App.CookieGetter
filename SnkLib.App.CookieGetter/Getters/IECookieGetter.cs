using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Runtime.InteropServices;

namespace SunokoLibrary.Application.Browsers
{
    /// <summary>
    /// IEやトライデントエンジンを利用しているブラウザのクッキーを取得する
    /// </summary>
    public class IECookieGetter : CookieGetterBase
    {
        public IECookieGetter(BrowserConfig option) : base(option, PathType.Directory) { }
        public override bool IsAvailable { get { return true; } }
        public override bool GetCookies(Uri targetUrl, System.Net.CookieContainer container)
        {
            string lpszCookieData;
            var hResult = Win32Api.GetCookiesFromIE(out lpszCookieData, targetUrl, null);
            Debug.WriteLineIf(lpszCookieData == null, "InternetGetCookie error code: " + hResult);

            if (lpszCookieData != null)
            {
                Debug.WriteLine(lpszCookieData);
                var cookies = new CookieCollection();
                foreach (var item in ParseCookies(lpszCookieData, targetUrl))
                    cookies.Add(item);
                container.Add(cookies);
                return true;
            }
            else
                return false;
        }
        public override ICookieImporter Generate(BrowserConfig config)
        { return new IECookieGetter(config); }

        /// <summary>
        /// 渡されたcookieヘッダーをcookieに変換する。CookieContainer.SetCookies()は不都合が生じる。
        /// </summary>
        protected List<Cookie> ParseCookies(string cookieHeader, Uri url)
        {
            var cookies = new List<Cookie>();
            if (string.IsNullOrEmpty(cookieHeader) == false)
            {
                Debug.WriteLine(cookieHeader);
                var cookieDatas = cookieHeader.ToString().Split(';');
                foreach (var data in cookieDatas)
                {
                    var cookie = new Cookie();
                    var chunks = data.ToString().Split('=');
                    if (2 > chunks.Length)
                        continue;
                    try
                    {
                        cookie.Name = chunks[0].Trim();
                        cookie.Value = chunks[1].Trim();
                        cookie.Domain = url.Host;
                        cookie.Path = url.Segments[0];    // このほうがいいきがする 2011-11-19
                        //cookie.Path = url.AbsolutePath;
                        cookie.Expires = DateTime.Now.AddDays(30);    // 有効期限適当付与 2013-07-03
                        cookies.Add(cookie);
                    }
                    catch (System.Net.CookieException e)
                    { Debug.WriteLine(e.Message); }
                }
            }

            return cookies;
        }
    }
}