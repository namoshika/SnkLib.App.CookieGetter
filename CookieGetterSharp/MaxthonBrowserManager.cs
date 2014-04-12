using System;
using System.Collections.Generic;
using System.Text;

namespace Hal.CookieGetterSharp {
	class MaxthonBrowserManager : IBrowserManager {
		const string COOKIEPATH = "%APPDATA%\\Maxthon3\\Users\\guest\\Cookie\\Cookie.dat";

		#region IBrowserManager メンバ

		public BrowserType BrowserType {
			get { return BrowserType.Maxthon; }
		}

		public ICookieGetter CreateDefaultCookieGetter() {
			string name = "Maxthon webkit";
			string path = Utility.ReplacePathSymbols(COOKIEPATH);

			if(!System.IO.File.Exists(path)) {
				path = null;
			}

			CookieStatus status = new CookieStatus(name, path, this.BrowserType, PathType.File);
			return new GoogleChromeCookieGetter(status);
		}

		public ICookieGetter[] CreateCookieGetters() {
			return new ICookieGetter[] { CreateDefaultCookieGetter() };
		}

		#endregion
	}
}
