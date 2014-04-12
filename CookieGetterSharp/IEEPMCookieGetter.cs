using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;

namespace Hal.CookieGetterSharp {

	/// <summary>
	/// 拡張保護モードIEブラウザのクッキーを取得する
	/// </summary>
	class IEEPMCookieGetter : AIECookieGetter {

		public IEEPMCookieGetter(CookieStatus status, bool checkSubDirectory)
			: base(status, checkSubDirectory) {
		}

		/// <summary>
		/// 対象URL上の名前がKeyであるクッキーを取得する
		/// </summary>
		/// <param name="url"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public override System.Net.Cookie GetCookie(Uri url, string key) {
			return base.GetCookie(url, key);
		}

		/// <summary>
		/// urlに関連付けられたクッキーを取得します。
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		public override System.Net.CookieCollection GetCookieCollection(Uri url) {
			return base.GetCookieCollection(url);
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
			// 拡張保護モードはAPIが使えないので何もしない
			return new List<System.Net.Cookie>();
		}

	}
}
