using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Hal.CookieGetterSharp {

	/// <summary>
	/// IEのクッキーのうちVista以降の保護モードで使われるクッキーのみを取得する
	/// </summary>
	class IESafemodeBrowserManager : IBrowserManager {
		#region IBrowserManager メンバ

		public BrowserType BrowserType {
			get { return BrowserType.IESafemode; }
		}

		public ICookieGetter CreateDefaultCookieGetter() {
			string cookieFolder = Environment.GetFolderPath(Environment.SpecialFolder.Cookies);
			string lowFolder = System.IO.Path.Combine(cookieFolder, "low");

			CookieStatus status = new CookieStatus(this.BrowserType.ToString(), lowFolder, this.BrowserType, PathType.Directory);
			return new IEPMCookieGetter(status, false);
		}

		/// <summary>
		/// IEBrowserManagerで環境にあわせて適切な物を返すようにしてあるので、ここでは何もしない
		/// </summary>
		/// <returns></returns>
		public ICookieGetter[] CreateCookieGetters() {
			return new ICookieGetter[0];
		}

		#endregion
	}
}
