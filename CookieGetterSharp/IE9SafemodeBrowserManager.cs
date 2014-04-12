using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Hal.CookieGetterSharp
{

	/// <summary>
	/// IEのクッキーのうちVista以降の保護モードで使われるクッキーのみを取得する
	/// </summary>
	class IE9SafemodeBrowserManager : IBrowserManager
	{
		#region IBrowserManager メンバ

		public BrowserType BrowserType
		{
			get { return BrowserType.IElSafemode; }
		}

		public ICookieGetter CreateDefaultCookieGetter() {
			string AppFolder = Environment.GetFolderPath(Environment.SpecialFolder.Cookies);
			AppFolder = Path.Combine(AppFolder, "Low");
		//	string AppFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"..\");
		//	string AppFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			CookieStatus status = new CookieStatus(this.BrowserType.ToString(), AppFolder, this.BrowserType, PathType.Directory);
			return new IE9CookieGetter(status, true);
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
