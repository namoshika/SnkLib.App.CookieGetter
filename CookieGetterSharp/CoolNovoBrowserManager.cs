using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Win32;


namespace Hal.CookieGetterSharp {
	class CoolNovoBrowserManager : IBrowserManager {
		const string COOKIEPATH = "%APPDATA%\\ChromePlus\\ChromePlusUserData\\Default\\Cookies";
		static readonly string NEWCOOKIEPATH = "%LOCALAPPDATA%\\MapleStudio\\ChromePlus\\User Data\\Default\\Cookies";	// 2011-12-07
		const string rKeyName = @"Software\ChromePlus";
		const string rGetValueName = "Install_Dir";

		// プロファイル対応 4/28
		private readonly string DATAFOLDER = null;
		private readonly string BaseFolder = "%LOCALAPPDATA%\\MapleStudio\\";
		private readonly string ProfileFolder = @"ChromePlus\User Data";
		private readonly string DefaultFolder = "Default";
		private readonly string ProfileFolderStarts = "Profile";
		private readonly string COOKEFILE_NAME = "Cookies";

		#region IBrowserManager メンバ

		public BrowserType BrowserType {
			get { return BrowserType.CoolNovo; }
		}

		public ICookieGetter CreateDefaultCookieGetter() {
			string name = string.Format("{0} {1}", BrowserType.ToString(), DefaultFolder);
			string path = null;
			if(DATAFOLDER != null) {
				path = Path.Combine(Path.Combine(Utility.ReplacePathSymbols(DATAFOLDER), DefaultFolder), COOKEFILE_NAME);
			}

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
			//return new ICookieGetter[] { CreateDefaultCookieGetter() };
		}

		#endregion

		public CoolNovoBrowserManager() {
			string folder = null;

			try {
				//Vista 7以外は別の場所になるので、インストールフォルダーを捜す
				// レジストリ・キーのパスを指定してレジストリを開く
				RegistryKey rKey = Registry.CurrentUser.OpenSubKey(rKeyName);
				if(rKey != null) {
					// レジストリの値を取得
					string location = (string)rKey.GetValue(rGetValueName);

					// 開いたレジストリ・キーを閉じる
					rKey.Close();

					// コンソールに取得したレジストリの値を表示
					if(location != null) {
						folder = Path.Combine(location, ProfileFolder);

						if(!System.IO.Directory.Exists(folder)) {
							folder = null;
						}
					}
				}
				else {
					folder = null;
				}
			}
			catch(TypeInitializationException) {
				folder = null;
			}
			catch(NullReferenceException) {
				folder = null;
			}

			if(folder == null) {
				folder = Path.Combine(Utility.ReplacePathSymbols(BaseFolder), ProfileFolder);
				if(!System.IO.Directory.Exists(folder)) {
					folder = null;
				}
			}

			DATAFOLDER = folder;
		}

		private string[] GetProfiles() {

			List<string> profiles = new List<string>();
			if(DATAFOLDER != null) {
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
