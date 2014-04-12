using System;
using System.Collections.Generic;
using System.Text;

namespace Hal.CookieGetterSharp
{
	/// <summary>
	/// CookieGetterを生成するためのインターフェース
	/// </summary>
	public interface IBrowserManager
	{
		/// <summary>
		/// ブラウザの種類
		/// </summary>
		BrowserType BrowserType { get; }

		/// <summary>
		/// 既定のCookieGetterを取得します
		/// </summary>
		/// <returns></returns>
		ICookieGetter CreateDefaultCookieGetter();

		/// <summary>
		/// 利用可能なすべてのCookieGetterを取得します
		/// </summary>
		/// <returns></returns>
		ICookieGetter[] CreateCookieGetters();
		
	}
}
