using System;
using System.Collections.Generic;
using System.Text;

namespace Hal.CookieGetterSharp
{

	/// <summary>
	/// IE系のすべてのクッキーを取得する
	/// </summary>
	class IEBrowserManager : IBrowserManager
	{
		#region IBrowserManager メンバ

		public BrowserType BrowserType
		{
			get { return BrowserType.IE; }
		}

		public ICookieGetter CreateDefaultCookieGetter()
		{
			string cookieFolder = Environment.GetFolderPath(Environment.SpecialFolder.Cookies);
			CookieStatus status = new CookieStatus(this.BrowserType.ToString(), cookieFolder, this.BrowserType, PathType.Directory);
			return new IECookieGetter(status, true);
		}

		public ICookieGetter[] CreateCookieGetters()
		{
			string cookieFolder = Environment.GetFolderPath(Environment.SpecialFolder.Cookies);

			string lowFolder = System.IO.Path.Combine(cookieFolder, "low");
			if (System.IO.Directory.Exists(lowFolder)) {
				IEComponentBrowserManager iec = new IEComponentBrowserManager();
				IESafemodeBrowserManager ies = new IESafemodeBrowserManager();
				IEEnhancedProtectedModeBrowserManager ieepm = new IEEnhancedProtectedModeBrowserManager();
				return new ICookieGetter[] { iec.CreateDefaultCookieGetter(), ies.CreateDefaultCookieGetter(), ieepm.CreateDefaultCookieGetter() };
			}
			else {
				return new ICookieGetter[] { CreateDefaultCookieGetter() };
			}
			/*
			string lowFolder = System.IO.Path.Combine(cookieFolder, "low");
			if (System.IO.Directory.Exists(lowFolder)) {
				IEComponentBrowserManager iec = new IEComponentBrowserManager();
				IESafemodeBrowserManager ies = new IESafemodeBrowserManager();
				IE9SafemodeBrowserManager ie9s = new IE9SafemodeBrowserManager();
				return new ICookieGetter[] { iec.CreateDefaultCookieGetter(), ies.CreateDefaultCookieGetter(), ie9s.CreateDefaultCookieGetter()};
			}
			else {
				return new ICookieGetter[] { CreateDefaultCookieGetter() };
			}
			*/
		}

		#endregion
	}
}
