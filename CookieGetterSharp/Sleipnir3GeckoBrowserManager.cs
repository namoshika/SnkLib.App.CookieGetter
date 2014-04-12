using System;
using System.Collections.Generic;
using System.Text;

namespace Hal.CookieGetterSharp {
	class Sleipnir3GeckoBrowserManager : IBrowserManager {
		const string DATAFOLDER = "%APPDATA%\\Fenrir Inc\\Sleipnir\\setting\\modules\\geckoviewer\\";
		const string COOKEFILE_NAME = "cookies.sqlite";

		#region IBrowserManager メンバ

		public BrowserType BrowserType {
			get { return BrowserType.Sleipnir3Gecko; }
		}

		public ICookieGetter CreateDefaultCookieGetter() {
			return CreateCookieGetter();
		}

		public ICookieGetter[] CreateCookieGetters() {
			return new ICookieGetter[] { CreateCookieGetter() };
		}

		#endregion

		private ICookieGetter CreateCookieGetter() {
			string name = "Sleipnir3 Gecko";
			string path = System.IO.Path.Combine(Utility.ReplacePathSymbols(DATAFOLDER), COOKEFILE_NAME);

			CookieStatus status = new CookieStatus(name, path, this.BrowserType, PathType.File);
			return new FirefoxCookieGetter(status);
		}
	}
}
