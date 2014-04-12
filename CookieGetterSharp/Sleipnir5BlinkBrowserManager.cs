using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Hal.CookieGetterSharp {
	class Sleipnir5BlinkBrowserManager : IBrowserManager {
		const string COOKIEPATH = "%APPDATA%\\Fenrir Inc\\Sleipnir5\\setting\\modules\\ChromiumViewer\\Default\\Cookies";

		// プロファイル対応 6/29
		private readonly string DATAFOLDER = "%APPDATA%\\Fenrir Inc\\Sleipnir5\\setting\\modules\\ChromiumViewer\\";
		private readonly string DefaultFolder = "Default";
		private readonly string ProfileFolderStarts = "Profile";
		private readonly string COOKEFILE_NAME = "Cookies";

		#region IBrowserManager メンバ

		public BrowserType BrowserType {
			get { return BrowserType.Sleipnir5Blink; }
		}

		public ICookieGetter CreateDefaultCookieGetter() {
			string name = string.Format("{0} {1}", BrowserType.ToString(), DefaultFolder);
			string path = Path.Combine(Path.Combine(Utility.ReplacePathSymbols(DATAFOLDER), DefaultFolder), COOKEFILE_NAME);

			if(!System.IO.File.Exists(path)) {
				path = null;
			}

			CookieStatus status = new CookieStatus(name, path, this.BrowserType, PathType.File);
			return new GoogleChromeCookieGetter(status);

		}

		public ICookieGetter[] CreateCookieGetters() {
			string[] profs = GetProfiles();
			if(profs.Length == 0) {
				return new ICookieGetter[] { CreateDefaultCookieGetter() };
			}

			List<ICookieGetter> cgs = new List<ICookieGetter>();
			cgs.Add(CreateDefaultCookieGetter());
			for(int i = 0;i < profs.Length;i++) {
				cgs.Add(CreateCookieGetter(profs[i]));
			}
			return cgs.ToArray();
		}

		#endregion

		private string[] GetProfiles() {
			List<string> profiles = new List<string>();
			string folder = Utility.ReplacePathSymbols(DATAFOLDER);
			if(Directory.Exists(folder)) {
				string[] path = Directory.GetDirectories(folder);
				for(int i = 0;i < path.Length;i++) {
					if(Path.GetFileName(path[i]).StartsWith(ProfileFolderStarts, StringComparison.OrdinalIgnoreCase)) {
						if(File.Exists(Path.Combine(path[i], COOKEFILE_NAME))) {
							profiles.Add(path[i]);
						}
					}
				}
			}

			return profiles.ToArray();
		}

		private ICookieGetter CreateCookieGetter(string prof) {
			string name = BrowserType.ToString();
			string path = null;

			if(prof != null) {
				name += " " + Path.GetFileName(prof);
				path = System.IO.Path.Combine(prof, COOKEFILE_NAME);
			}

			CookieStatus status = new CookieStatus(name, path, this.BrowserType, PathType.File);
			return new GoogleChromeCookieGetter(status);
		}

	}
}
