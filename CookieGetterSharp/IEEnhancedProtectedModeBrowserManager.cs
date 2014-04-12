using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Hal.CookieGetterSharp
{

	/// <summary>
	/// IEのクッキーのうち8以降の拡張保護モードで使われるクッキーのみを取得する
	/// </summary>
	class IEEnhancedProtectedModeBrowserManager : IBrowserManager {
		private readonly string COOKIEPATH = "%LOCALAPPDATA%\\Packages\\windows_ie_ac_001\\AC\\INetCookies";

		#region IBrowserManager メンバ

		public BrowserType BrowserType
		{
			get { return BrowserType.IEEPMode; }
		}

		public ICookieGetter CreateDefaultCookieGetter() {
			string lowFolder = null;

			using(Microsoft.Win32.RegistryKey regkey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Internet Explorer\LowRegistry\IEShims\NormalizedPaths", false)) {
				if(regkey != null) {
					string[] valueNames = regkey.GetValueNames();
					string w81UpgradePath = null;
					foreach(string value in valueNames) {
						if(value.EndsWith("temp", StringComparison.CurrentCultureIgnoreCase)) {
							w81UpgradePath = value;
						}
					}
					if(!string.IsNullOrEmpty(w81UpgradePath)) {
						lowFolder = Path.Combine(Path.GetDirectoryName(w81UpgradePath), "INetCookies");
					}
				}
			}

			if(string.IsNullOrEmpty(lowFolder)) {
				string path = Utility.ReplacePathSymbols(COOKIEPATH);
				try {
					if(0 < System.IO.Directory.GetFiles(path).Length) {
						lowFolder = path;
					}
				}
				catch { }
			}

			if(string.IsNullOrEmpty(lowFolder)) {
				string cookieFolder = Environment.GetFolderPath(Environment.SpecialFolder.Cookies);
				lowFolder = System.IO.Path.Combine(cookieFolder, "low");
			}

			CookieStatus status = new CookieStatus(this.BrowserType.ToString(), lowFolder, this.BrowserType, PathType.Directory);
			return new IEEPMCookieGetter(status, false);
		}

		/// <summary>
		/// IEBrowserManagerで環境にあわせて適切な物を返すようにしてあるので、ここでは何もしない
		/// </summary>
		/// <returns></returns>
		public ICookieGetter[] CreateCookieGetters()
		{
			return new ICookieGetter[0];
		}

		#endregion
	}
}
