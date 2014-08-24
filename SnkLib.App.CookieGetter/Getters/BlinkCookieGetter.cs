using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SunokoLibrary.Application.Browsers
{
    /// <summary>
    /// GoogleChromeからクッキーを取得する
    /// </summary>
    public class BlinkCookieGetter : WebkitCookieGetter
    {
        public BlinkCookieGetter(BrowserConfig status) : base(status) { }
        const string SELECT_QUERY = "SELECT value, name, host_key, path, expires_utc FROM cookies";

        public override ICookieImporter Generate(BrowserConfig config)
        { return new BlinkCookieGetter(config); }
        protected override Cookie DataToCookie(object[] data)
        {
            var cookie = new System.Net.Cookie();
            var value = data[0] as string;
            // なんというめちゃくちゃな
            // chrome cookie version 7
            if (data.Length == 6)
                try
                {
                    var cipher = data[5] as byte[];
                    if (cipher == null || cipher.Length == 0)
                        cookie.Value = data[0] as string;
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
                            return null;

                        //リソース解放
                        Debug.WriteLineIf(isSucc, Config.ProfileName + ": CryptUnprotectData ok");
                        var plain = new byte[output.cbData];
                        Marshal.Copy(output.pbData, plain, 0, (int)output.cbData);
                        cookie.Value = Encoding.UTF8.GetString(plain);
                        //output解放
                        isSucc = Win32Api.LocalFree(output.pbData) == IntPtr.Zero;
                        Debug.Assert(isSucc, "CryptUnprotectData error: BlinkのCookie復号後のoutputリソース解放に失敗。");
                        Debug.WriteLineIf(isSucc, Config.ProfileName + ": output.pbData free");
                        //input解放
                        isSucc = Win32Api.LocalFree(input.pbData) == IntPtr.Zero;
                        Debug.Assert(isSucc, "CryptUnprotectData error: BlinkのCookie復号後のinputリソース解放に失敗。");
                        Debug.WriteLineIf(isSucc, Config.ProfileName + ": input.pbData free");
                    }
                }
                catch { return null; }
            else
                cookie.Value = data[0] as string;

            //cookie.valueはnull以外である必要がある。空文字でも弾く必要があるかは分からない。
            //Geckoの方で空文字が出てきて不都合が生じるケースがあるのでここでは弾く
            if (string.IsNullOrEmpty(cookie.Value))
                return null;

            cookie.Name = data[1] as string;
            cookie.Domain = data[2] as string;
            cookie.Path = data[3] as string;
            cookie.Value = cookie.Value != null
                ? cookie.Value.Replace(";", "%3b").Replace(",", "%2c") : null;

            try
            {
                var exp = long.Parse(data[4].ToString());
                // クッキー有効期限が正確に取得されていなかったので修正
                cookie.Expires = Utility.UnixTimeToDateTime((int)((long)(exp / 1000000) - 11644473600));
            }
            catch (Exception ex)
            { throw new CookieImportException("GoogleChromeのexpires変換に失敗しました", ex); }

            return cookie;
        }
        protected override string MakeQuery(Uri url)
        { return string.Format("{0} {1} ORDER BY creation_utc DESC", SELECT_QUERY, MakeWhere(url)); }
    }
}
