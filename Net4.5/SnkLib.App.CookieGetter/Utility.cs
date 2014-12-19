using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;

namespace SunokoLibrary.Application
{
    static class Utility
    {
        static DateTime unix = new DateTime(1970, 1, 1, 9, 0, 0);
        public static DateTime UnixTimeToDateTime(ulong UnixTime)
        { return unix.AddSeconds(UnixTime); }
        public static ulong DateTimeToUnixTime(DateTime dateTime)
        { return (ulong)(dateTime - unix).TotalSeconds; }
        /// <summary>
        /// %APPDATA%などを実際のパスに変換する
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string ReplacePathSymbols(string path)
        {
            path = path.Replace("%APPDATA%", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
            path = path.Replace("%LOCALAPPDATA%", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
            path = path.Replace("%COOKIES%", Environment.GetFolderPath(Environment.SpecialFolder.Cookies));
            return path;
        }
    }
    static class Win32Api
    {
        /// <summary>
        /// CryptProtectDataでデータを暗号化する。
        /// </summary>
        /// <param name="cipher">処理対象のデータ</param>
        /// <returns>暗号化されたデータ</returns>
        public static byte[] CryptProtectedData(byte[] cipher)
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
            isSucc = Win32Api.CryptProtectData(ref input, null, ref dammy, IntPtr.Zero, IntPtr.Zero, 0, ref output);
            Debug.Assert(isSucc, "CryptUnprotectData error: データ暗号化に失敗。");
            if (isSucc == false)
                return null;

            //リソース解放
            var decryptedBytes = new byte[output.cbData];
            Marshal.Copy(output.pbData, decryptedBytes, 0, (int)output.cbData);
            //output解放
            isSucc = Win32Api.LocalFree(output.pbData) == IntPtr.Zero;
            Debug.Assert(isSucc, "CryptUnprotectData error: データ暗号化後のoutputリソース解放に失敗。");
            //input解放
            isSucc = Win32Api.LocalFree(input.pbData) == IntPtr.Zero;
            Debug.Assert(isSucc, "CryptUnprotectData error: データ暗号化後のinputリソース解放に失敗。");

            return decryptedBytes;
        }
        /// <summary>
        /// CryptUnprotectDataで暗号化されたデータを復号化する。
        /// </summary>
        /// <param name="cipher">暗号化されたデータ</param>
        /// <returns>復号化されたデータ</returns>
        public static byte[] DecryptProtectedData(byte[] cipher)
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
            Debug.Assert(isSucc, "CryptUnprotectData error: データ復号に失敗。");
            if (isSucc == false)
                return null;

            //リソース解放
            var decryptedBytes = new byte[output.cbData];
            Marshal.Copy(output.pbData, decryptedBytes, 0, (int)output.cbData);
            //output解放
            isSucc = Win32Api.LocalFree(output.pbData) == IntPtr.Zero;
            Debug.Assert(isSucc, "CryptUnprotectData error: データ復号後のoutputリソース解放に失敗。");
            //input解放
            isSucc = Win32Api.LocalFree(input.pbData) == IntPtr.Zero;
            Debug.Assert(isSucc, "CryptUnprotectData error: データ復号後のinputリソース解放に失敗。");

            return decryptedBytes;
        }
        /// <summary>
        /// 保護モードIEからCookieを取得する
        /// </summary>
        /// <param name="cookiesText">取得したCookieの代入先</param>
        /// <param name="targetUrl">Cookieを送りたいページのURL</param>
        /// <param name="valueKey">読み出したいCookieのキー値</param>
        /// <returns>引数targetUrlに対して使えるCookieヘッダー値</returns>
        public static int GetCookiesFromProtectedModeIE(out string cookiesText, Uri targetUrl, string valueKey = null)
        {
            var cookieSize = 4096;
            var lpszCookieData = new StringBuilder(cookieSize);
            var dwSizeP = new IntPtr(cookieSize);

            do
            {
                var hResult = IEGetProtectedModeCookie(
                    targetUrl.OriginalString, valueKey, lpszCookieData, ref cookieSize, INTERNET_COOKIE_HTTPONLY);
                switch ((uint)hResult)
                {
                    case 0x8007007A://バッファー不足
                        cookieSize = cookieSize + 256;
                        lpszCookieData.Capacity = cookieSize;
                        continue;
                    case 0x00000000://S_OK
                    case 0x80070103://データ無し
                    case 0x80070057://E_INVALIDARG: IEが非保護モードだと出てきたりする
                        cookiesText = lpszCookieData.ToString();
                        return hResult;
                    default:
                        cookiesText = null;
                        return hResult;
                }
            }
            while (true);
        }
        /// <summary>
        /// 従来モードIEからCookieを取得する
        /// </summary>
        /// <param name="cookiesText">取得したCookieの代入先</param>
        /// <param name="targetUrl">Cookieを送りたいページのURL</param>
        /// <param name="valueKey">読み出したいCookieのキー値</param>
        /// <returns>引数targetUrlに対して使えるCookieヘッダー値</returns>
        public static int GetCookiesFromIE(out string cookiesText, Uri targetUrl, string valueKey = null)
        {
            var cookieSize = 4096;
            var lpszCookieData = new StringBuilder(cookieSize);
            var dwSizeP = new IntPtr(cookieSize);

            //クッキー文字列取得
            do
            {
                bool bResult;
                if (bResult = Win32Api.InternetGetCookieEx(targetUrl.OriginalString, valueKey, lpszCookieData, ref cookieSize, Win32Api.INTERNET_COOKIE_HTTPONLY, IntPtr.Zero))
                {
                    cookiesText = lpszCookieData.ToString();
                    return 0x00000000;
                }
                else
                {
                    var errorNo = Marshal.GetHRForLastWin32Error();
                    switch ((uint)errorNo)
                    {
                        case 0x00000000:
                        case 0x80070103:
                            cookiesText = lpszCookieData.ToString();
                            return errorNo;
                        case 0x8007007A:
                            //バッファーサイズ拡張。無限ループが怖いので一応必ずサイズが増えるようにしておく
                            cookieSize += 512;
                            lpszCookieData.Capacity = cookieSize;
                            continue;
                        default:
                            cookiesText = null;
                            return errorNo;
                    }
                }
            }
            while (true);
        }
        /// <summary>
        /// IEのバージョンを取得する
        /// </summary>
        /// <returns>ex:11.0.9600.17239</returns>
        public static Version GetIEVersion()
        {
            try
            {
                using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Internet Explorer"))
                    return Version.Parse((string)key.GetValue("svcVersion") ?? (string)key.GetValue("Version"));
            }
            catch { return null; }
        }

        const uint INTERNET_COOKIE_THIRD_PARTY = 0x00000010;
        const uint INTERNET_COOKIE_HTTPONLY = 0x00002000;
        const uint INTERNET_FLAG_RESTRICTED_ZONE = 0x00020000;
        [DllImport("Wininet", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool InternetGetCookieEx(string lpszURL, string lpszCookieName, StringBuilder lpszCookieData, ref int lpdwSize, uint dwFlags, IntPtr lpReserved);
        [DllImport("Wininet", CharSet = CharSet.Unicode)]
        static extern bool InternetGetCookie(string lpszURL, string lpszCookieName, StringBuilder lpszCookieData, ref uint lpdwSize);
        [DllImport("ieframe.dll", CharSet = CharSet.Unicode)]
        static extern int IEGetProtectedModeCookie(string lpszURL, string lpszCookieName, StringBuilder pszCookieData, ref int pcchCookieData, uint dwFlags);

        [StructLayout(LayoutKind.Sequential)]
        struct DATA_BLOB
        {
            public uint cbData;
            public IntPtr pbData;
        }

        [DllImport("Crypt32.dll", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode)]
        static extern bool CryptProtectData(ref DATA_BLOB pDataIn, string ppszDataDescr, ref  DATA_BLOB pOptionalEntropy, IntPtr pvReserved, IntPtr pPromptStruct, uint dwFlags, [In, Out]ref DATA_BLOB pDataOut);
        [DllImport("Crypt32.dll", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode)]
        static extern bool CryptUnprotectData(ref DATA_BLOB pDataIn, string ppszDataDescr, ref  DATA_BLOB pOptionalEntropy, IntPtr pvReserved, IntPtr pPromptStruct, uint dwFlags, [In, Out]ref DATA_BLOB pDataOut);
        [DllImport("Kernel32.dll")]
        static extern IntPtr LocalFree(IntPtr hMem);
    }
}