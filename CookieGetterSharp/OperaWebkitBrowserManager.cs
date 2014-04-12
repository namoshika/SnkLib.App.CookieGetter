using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Hal.CookieGetterSharp {
	class OperaWebkitBrowserManager : IBrowserManager {
		private readonly string DATAFOLDER = "%APPDATA%\\Opera Software\\Opera Stable";
		private readonly string COOKEFILE_NAME = "Cookies";

		#region IBrowserManager ÉÅÉìÉo

		public BrowserType BrowserType {
			get { return BrowserType.OperaWebkit; }
		}

		public ICookieGetter CreateDefaultCookieGetter() {

			string folder = Path.Combine(Utility.ReplacePathSymbols(DATAFOLDER), COOKEFILE_NAME);
			CookieStatus status = new CookieStatus("Opera Webkit", folder, this.BrowserType, PathType.File);
			return new GoogleChromeCookieGetter(status);
		}

		public ICookieGetter[] CreateCookieGetters() {
			return new ICookieGetter[] { CreateDefaultCookieGetter() };
		}

		#endregion

	}
}
