using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace SunokoLibrary.Application.Browsers
{
    /// <summary>
    /// Firefoxからクッキーを取得する
    /// </summary>
    public class GeckoCookieGetter : SqlCookieGetter
    {
        public GeckoCookieGetter(BrowserConfig status) : base(status) { }
        const string SELECT_QUERY = "SELECT value, name, host, path, expiry FROM moz_cookies";

        public override bool GetCookies(Uri targetUrl, System.Net.CookieContainer container)
        {
            if (IsAvailable == false)
                return false;
            try
            {
                container.Add(LookupCookies(Config.CookiePath, MakeQuery(targetUrl)));
                return true;
            }
            catch { return false; }
        }
        public override ICookieImporter Generate(BrowserConfig config)
        { return new GeckoCookieGetter(config); }
        protected override Cookie DataToCookie(object[] data)
        {
            var cookie = new Cookie();
            if (string.IsNullOrEmpty((string)data[0]) || string.IsNullOrEmpty((string)data[1]))
                return null;
            cookie.Value = Uri.UnescapeDataString((string)data[0]).Replace(";", "%3b").Replace(",", "%2c");
            cookie.Name = data[1] as string;
            cookie.Domain = data[2] as string;
            cookie.Path = data[3] as string;

            try
            {
                long exp = long.Parse(data[4].ToString());
                cookie.Expires = Utility.UnixTimeToDateTime((int)exp);
            }
            catch (Exception ex)
            { throw new CookieImportException("Firefoxのexpires変換に失敗しました", ex); }

            return cookie;
        }
        protected override string MakeQuery(Uri url)
        { return string.Format("{0} {1} ORDER BY expiry", SELECT_QUERY, MakeWhere(url)); }
        protected string MakeWhere(Uri url)
        {
            Stack<string> hostStack = new Stack<string>(url.Host.Split('.'));
            StringBuilder hostBuilder = new StringBuilder('.' + hostStack.Pop());
            string[] pathes = url.Segments;

            StringBuilder sb = new StringBuilder();
            sb.Append("WHERE (");

            bool needOr = false;
            while (hostStack.Count != 0)
            {
                if (needOr)
                {
                    sb.Append(" OR");
                }

                if (hostStack.Count != 1)
                {
                    hostBuilder.Insert(0, '.' + hostStack.Pop());
                    sb.AppendFormat(" host = \"{0}\"", hostBuilder.ToString());
                }
                else
                {
                    hostBuilder.Insert(0, '%' + hostStack.Pop());
                    sb.AppendFormat(" host LIKE \"{0}\"", hostBuilder.ToString());
                }

                needOr = true;
            }

            sb.Append(')');
            return sb.ToString();
        }
    }
}