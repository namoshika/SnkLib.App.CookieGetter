using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Hal.CookieGetterSharp {
	class TungstenBrowserManager : IBrowserManager {
		private readonly string DATAFOLDER = "%APPDATA%\\Tungsten\\profile\\Default";
		private readonly string COOKEFILE_NAME = "Cookies";

		#region IBrowserManager ÉÅÉìÉo

		public BrowserType BrowserType {
			get { return BrowserType.TungstenBlink; }
		}

		public ICookieGetter CreateDefaultCookieGetter() {

			string folder = Path.Combine(Utility.ReplacePathSymbols(DATAFOLDER), COOKEFILE_NAME);
			CookieStatus status = new CookieStatus("TungstenBlink", folder, this.BrowserType, PathType.File);
			return new GoogleChromeCookieGetter(status);
		}

		public ICookieGetter[] CreateCookieGetters() {
			return new ICookieGetter[] { CreateDefaultCookieGetter() };
		}

		#endregion

	}
}
