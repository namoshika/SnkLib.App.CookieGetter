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
    /// Firefoxからクッキーを取得する
    /// </summary>
    public class GeckoCookieGetter : SqlCookieGetter
    {
        public GeckoCookieGetter(BrowserConfig config) : base(config) { }
        const string SELECT_QUERY = "SELECT value, name, host, path, expiry FROM moz_cookies";

        public override ICookieImporter Generate(BrowserConfig config)
        { return new GeckoCookieGetter(config); }
        protected override async Task<ImportResult> ProtectedGetCookiesAsync(Uri targetUrl, System.Net.CookieContainer container)
        {
            if (IsAvailable == false)
                return ImportResult.Unavailable;
            try
            {
                var query = string.Format("{0} {1} ORDER BY expiry", SELECT_QUERY, MakeWhere(targetUrl));
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
            if (data.Length < 5 || data.Take(4).Where(rec => rec is string == false).Any() || data[4] is long == false)
                throw new CookieImportException(
                    "レコードからCookieオブジェクトへの変換に失敗しました。", ImportResult.ConvertError);
            if (string.IsNullOrEmpty((string)data[0]) || string.IsNullOrEmpty((string)data[1]))
                return null;

            var cookie = new Cookie()
            {
                Value = Uri.UnescapeDataString((string)data[0]).Replace(";", "%3b").Replace(",", "%2c"),
                Name = (string)data[1],
                Domain = (string)data[2],
                Path = (string)data[3],
                Expires = Utility.UnixTimeToDateTime((int)(long)data[4]),
            };
            return cookie;
        }
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