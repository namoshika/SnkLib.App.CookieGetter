using System;
using System.Collections.Generic;
using System.Text;

namespace Hal.CookieGetterSharp
{
	class Operax64BrowserManager : IBrowserManager
	{
		
		const string COOKIEPATH = "%APPDATA%\\Opera\\Opera x64\\cookies4.dat";

		#region IBrowserManager メンバ

		public BrowserType BrowserType
		{
			get { return BrowserType.Opera64; }
		}

		public ICookieGetter CreateDefaultCookieGetter()
		{
			string path = Utility.ReplacePathSymbols(COOKIEPATH);

			if (!System.IO.File.Exists(path)) {
				path = null;
			}

			CookieStatus status = new CookieStatus(this.BrowserType.ToString(), path, this.BrowserType, PathType.File);
			return new OperaCookieGetter(status);
		}

		public ICookieGetter[] CreateCookieGetters()
		{
			return new ICookieGetter[] { CreateDefaultCookieGetter() };
		}

		#endregion
	}
}
