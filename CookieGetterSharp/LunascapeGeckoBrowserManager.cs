using System;
using System.Collections.Generic;
using System.Text;

namespace Hal.CookieGetterSharp
{
	class LunascapeGeckoBrowserManager : IBrowserManager {
		const string LUNASCAPE_PLUGIN_FOLDER5 = "%APPDATA%\\Lunascape\\Lunascape5\\ApplicationData\\gecko\\cookies.sqlite";
		const string LUNASCAPE_PLUGIN_FOLDER6 = "%APPDATA%\\Lunascape\\Lunascape6\\plugins";
		const string COOKIEPATH = "data\\cookies.sqlite";

		#region IBrowserManager メンバ

		public BrowserType BrowserType
		{
			get { return BrowserType.LunascapeGecko; }
		}

		public ICookieGetter CreateDefaultCookieGetter()
		{
			string path = SearchDirectory();

			CookieStatus status = new CookieStatus("Lunascape Gecko", path, this.BrowserType, PathType.File);
			return new FirefoxCookieGetter(status);
		}

		public ICookieGetter[] CreateCookieGetters()
		{
			return new ICookieGetter[] { CreateDefaultCookieGetter() };
		}

		#endregion

		/// <summary>
		/// Lunascape6のプラグインフォルダからFirefoxのクッキーが保存されているパスを検索する
		/// </summary>
		/// <returns></returns>
		private string SearchDirectory() {

			string path5 = Utility.ReplacePathSymbols(LUNASCAPE_PLUGIN_FOLDER5);
			if(System.IO.File.Exists(path5)) {
				return path5;
			}

			string dir = Utility.ReplacePathSymbols(LUNASCAPE_PLUGIN_FOLDER6);
			if (System.IO.Directory.Exists(dir)) {
				foreach (string folder in System.IO.Directory.GetDirectories(dir)) {
					string path = System.IO.Path.Combine(folder, COOKIEPATH);
					if (System.IO.File.Exists(path)) {
						return path;
					}
				}
			}
			return null;
		}
	}
}
