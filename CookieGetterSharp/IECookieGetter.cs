using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;

namespace Hal.CookieGetterSharp {

	/// <summary>
	/// ノーマルIEやトライデントエンジンを利用しているブラウザのクッキーを取得する
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

			int cookieMaxSize = 4096;
			StringBuilder lpszCookieData = new StringBuilder(cookieMaxSize);
			uint dwSize = (uint)lpszCookieData.Capacity;
			IntPtr dwSizeP = new IntPtr(cookieMaxSize);

			try {
				bool bResult = win32api.InternetGetCookie(url.OriginalString, key, lpszCookieData, ref dwSize);
				if(!bResult) {
					uint errorNo = win32api.GetLastError();
					Debug.WriteLine("InternetGetCookie error code: " + errorNo);
				}
			}
			catch(Exception e) {
				Debug.WriteLine(e.Message);
			}

			if(lpszCookieData.Length != 0 && lpszCookieData.Length < 4096) {
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
