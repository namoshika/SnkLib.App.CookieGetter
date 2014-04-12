using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;

namespace Hal.CookieGetterSharp {

	/// <summary>
    /// IEやトライデントエンジンを利用しているブラウザのクッキーを取得する
	/// </summary>
	class IECookieGetter : AIECookieGetter {
		public IECookieGetter(CookieStatus status, bool checkSubDirectory)
			: base(status, checkSubDirectory) {
		}

		/// <summary>
		/// 対象URL上の名前がKeyであるクッキーを取得する
		/// </summary>
		/// <param name="url"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public override System.Net.Cookie GetCookie(Uri url, string key) {
			{
				// win32apiを使いパフォーマンスを向上
				List<System.Net.Cookie> cookieDatas = GetCookiesWinApi(url, key);
				if(cookieDatas.Count == 1) {
					return cookieDatas.ToArray()[0];
				}
			}

			return base.GetCookie(url, key);
		}

		/// <summary>
		/// urlに関連付けられたクッキーを取得します。
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		public override System.Net.CookieCollection GetCookieCollection(Uri url) {
			// win32apiを使いパフォーマンスを向上
			List<System.Net.Cookie> cookies = GetCookiesWinApi(url, null);

			if(cookies.Count == 0) {
				//関係のあるファイルだけ調べることによってパフォーマンスを向上させる
				List<string> files = SelectFiles(url, GetAllFiles());

				cookies = new List<System.Net.Cookie>();
				foreach(string filepath in files) {
					cookies.AddRange(PickCookiesFromFile(filepath));
				}
			}

			// Expiresが最新のもで上書きする
			cookies.Sort(CompareCookieExpiresAsc);
			System.Net.CookieCollection collection = new System.Net.CookieCollection();
			foreach(System.Net.Cookie cookie in cookies) {
				try {
					collection.Add(cookie);
				}
				catch(Exception ex) {
					CookieGetter.Exceptions.Enqueue(ex);
					System.Diagnostics.Debug.WriteLine(ex.Message);
				}
			}

			return collection;
		}

		public override System.Net.CookieContainer GetAllCookies() {
			return base.GetAllCookies();
		}

		/// <summary>
		/// win32apiを使って対象URL上の名前がKeyであるクッキーを取得します
		/// </summary>
		/// <param name="url"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		protected override List<System.Net.Cookie> GetCookiesWinApi(Uri url, string key) {
			List<System.Net.Cookie> cookies = new List<System.Net.Cookie>();

			int cookieSize = 8;
			StringBuilder lpszCookieData = new StringBuilder(cookieSize);
			IntPtr dwSizeP = new IntPtr(cookieSize);

            //クッキー文字列取得
            do {
                bool bResult;
                if(bResult = win32api.InternetGetCookieEx(url.OriginalString, key, lpszCookieData, ref cookieSize, win32api.INTERNET_COOKIE_HTTPONLY, IntPtr.Zero))
                    break;
                else {
                    int errorNo = Marshal.GetHRForLastWin32Error();
                    switch((uint)errorNo) {
                        case 0x00000000:
                        case 0x80070103:
                            break;
                        case 0x8007007A:
                            //バッファーサイズ拡張。無限ループが怖いので一応必ずサイズが増えるようにしておく
                            cookieSize += 512;
                            lpszCookieData.Capacity = cookieSize;
                            continue;
                        default:
                            Debug.WriteLine("InternetGetCookie error code: " + errorNo);
                            break;
                    }
                }
                break;
            }
            while(true);

            //オブジェクト化
			if(lpszCookieData.Length != 0) {
				Debug.WriteLine(lpszCookieData);
				string[] cookieDatas = lpszCookieData.ToString().Split(';');
				foreach(var data in cookieDatas) {
					System.Net.Cookie cookie = new System.Net.Cookie();
					string[] chunks = data.ToString().Split('=');
					if(2 <= chunks.Length) {
						try {
							cookie.Name = chunks[0].Trim();
							cookie.Value = chunks[1].Trim();
							cookie.Domain = AddDotDomain(url.Host);
							cookie.Path = url.Segments[0];	// このほうがいいきがする 2011-11-19
							// cookie.Path = url.AbsolutePath;
							cookie.Expires = DateTime.Now.AddDays(30);	// 有効期限適当付与 2013-07-03
							cookies.Add(cookie);
						}
						catch(System.Net.CookieException e) {
							Debug.WriteLine(e.Message);
						}
					}
				}
			}

			return cookies;
		}

	}
}
