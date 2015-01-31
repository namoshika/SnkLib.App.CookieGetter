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
    internal static class Utility
    {
        static DateTime unix = new DateTime(1970, 1, 1, 9, 0, 0);
        public static DateTime UnixTimeToDateTime(ulong UnixTime)
        { return unix.AddSeconds(UnixTime); }
        public static ulong DateTimeToUnixTime(DateTime dateTime)
        { return (ulong)(dateTime - unix).TotalSeconds; }
        /// <summary>
        /// %APPDATA%などを実際のパスに変換します。
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
    internal static class Win32Api
    {
        public const uint INTERNET_COOKIE_THIRD_PARTY = 0x00000010;
        public const uint INTERNET_COOKIE_HTTPONLY = 0x00002000;
        public const uint INTERNET_FLAG_RESTRICTED_ZONE = 0x00020000;

        /// <summary>
        /// CryptProtectDataでデータを暗号化します。
        /// </summary>
        /// <param name="unencryptedData">処理対象のデータ</param>
        /// <returns>暗号化されたデータ</returns>
        public static byte[] CryptProtectedData(byte[] unencryptedData)
        {
            //リソース確保
            Win32Api.DATA_BLOB input;
            input.pbData = Marshal.AllocHGlobal(unencryptedData.Length);
            input.cbData = (uint)unencryptedData.Length;
            Marshal.Copy(unencryptedData, 0, input.pbData, unencryptedData.Length);
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
        /// CryptUnprotectDataで暗号化されたデータを復号化します。
        /// </summary>
        /// <param name="encryptedData">暗号化されたデータ</param>
        /// <returns>復号化されたデータ</returns>
        public static byte[] DecryptProtectedData(byte[] encryptedData)
        {
            //リソース確保
            Win32Api.DATA_BLOB input;
            input.pbData = Marshal.AllocHGlobal(encryptedData.Length);
            input.cbData = (uint)encryptedData.Length;
            Marshal.Copy(encryptedData, 0, input.pbData, encryptedData.Length);
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
        /// 保護モードIEからCookieを取得します。
        /// </summary>
        /// <param name="cookiesText">取得したCookieの代入先</param>
        /// <param name="targetUrl">Cookieを送りたいページのURL</param>
        /// <param name="valueKey">読み出したいCookieのキー値</param>
        /// <param name="paramsFlag">取得するCookieの範囲フラグ</param>
        /// <returns>引数targetUrlに対して使えるCookieヘッダー値</returns>
        public static int GetCookiesFromProtectedModeIE(out string cookiesText, Uri targetUrl, string valueKey = null, uint paramsFlag = INTERNET_COOKIE_HTTPONLY)
        {
            var cookieSize = 4096;
            var lpszCookieData = new StringBuilder(cookieSize);
            var dwSizeP = new IntPtr(cookieSize);

            cookiesText = null;
            for (int i = 0; ; i++)
            {
                var hResult = IEGetProtectedModeCookie(
                    targetUrl.OriginalString, valueKey, lpszCookieData, ref cookieSize, paramsFlag);
                switch ((uint)hResult)
                {
                    case 0x8007007A://バッファー不足
                        if (i >= 10)
                        {
                            Trace.Fail(
                                "SnkLib.App.CookieGetter error",
                                "GetCookiesFromProtectedModeIE()でエラーが発生しました。取得するCookieのサイズが想定サイズを超えています。");
                            return hResult;
                        }
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
        }
        /// <summary>
        /// 従来モードIEからCookieを取得します。
        /// </summary>
        /// <param name="cookiesText">取得したCookieの代入先</param>
        /// <param name="targetUrl">Cookieを送りたいページのURL</param>
        /// <param name="valueKey">読み出したいCookieのキー値</param>
        /// <param name="paramsFlag">取得するCookieの範囲フラグ</param>
        /// <returns>引数targetUrlに対して使えるCookieヘッダー値</returns>
        public static int GetCookiesFromIE(out string cookiesText, Uri targetUrl, string valueKey = null)
        {
            var cookieSize = 4096;
            var lpszCookieData = new StringBuilder(cookieSize);
            var dwSizeP = new IntPtr(cookieSize);

            //Cookie文字列取得
            cookiesText = null;
            for (int i = 0; ; i++)
            {
                bool bResult;
                if (bResult = Win32Api.InternetGetCookieEx(targetUrl.OriginalString, valueKey, lpszCookieData, ref cookieSize, Win32Api.INTERNET_COOKIE_HTTPONLY, IntPtr.Zero))
                {
                    cookiesText = lpszCookieData.ToString();
                    return 0x00000000;
                }
                //Errorが出ていた時
                var hResult = Marshal.GetHRForLastWin32Error();
                switch ((uint)hResult)
                {
                    case 0x8007007A: //バッファー不足
                        if (i >= 10)
                        {
                            Trace.Fail(
                                "SnkLib.App.CookieGetter error",
                                "GetCookiesFromIE()でエラーが発生しました。取得するCookieのサイズが想定サイズを超えています。");
                            return hResult;
                        }
                        cookieSize += 512;
                        lpszCookieData.Capacity = cookieSize;
                        continue;
                    case 0x00000000: //S_OK
                    case 0x80070103: //データ無し
                        cookiesText = lpszCookieData.ToString();
                        return hResult;
                    default:
                        cookiesText = null;
                        return hResult;
                }
            }
        }
        /// <summary>
        /// IEのバージョンを取得します。
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

        [DllImport("Wininet", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool InternetGetCookieEx(string lpszURL, string lpszCookieName, StringBuilder lpszCookieData, ref int lpdwSize, uint dwFlags, IntPtr lpReserved);
        [DllImport("Wininet", CharSet = CharSet.Unicode)]
        static extern bool InternetGetCookie(string lpszURL, string lpszCookieName, StringBuilder lpszCookieData, ref uint lpdwSize);
        [DllImport("ieframe.dll", CharSet = CharSet.Unicode)]
        static extern int IEGetProtectedModeCookie(string lpszURL, string lpszCookieName, StringBuilder pszCookieData, ref int pcchCookieData, uint dwFlags);
        [DllImport("Crypt32.dll", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode)]
        static extern bool CryptProtectData(ref DATA_BLOB pDataIn, string ppszDataDescr, ref  DATA_BLOB pOptionalEntropy, IntPtr pvReserved, IntPtr pPromptStruct, uint dwFlags, [In, Out]ref DATA_BLOB pDataOut);
        [DllImport("Crypt32.dll", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode)]
        static extern bool CryptUnprotectData(ref DATA_BLOB pDataIn, string ppszDataDescr, ref  DATA_BLOB pOptionalEntropy, IntPtr pvReserved, IntPtr pPromptStruct, uint dwFlags, [In, Out]ref DATA_BLOB pDataOut);
        [DllImport("Kernel32.dll")]
        static extern IntPtr LocalFree(IntPtr hMem);

        [StructLayout(LayoutKind.Sequential)]
        struct DATA_BLOB
        {
            public uint cbData;
            public IntPtr pbData;
        }
    }
}