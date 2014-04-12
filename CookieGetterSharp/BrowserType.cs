using System;
using System.Collections.Generic;
using System.Text;

namespace Hal.CookieGetterSharp
{
	/// <summary>
	/// ブラウザの種類
	/// </summary>
	public enum BrowserType
	{

		/// <summary>
		/// IE系ブラウザ(IEComponent + IESafemode)(XP)
		/// </summary>
		IE,

		/// <summary>
		/// XPのIEやトライデントエンジンを使用しているブラウザ
		/// </summary>
		IEComponent,

		/// <summary>
		/// Vista以降のIE
		/// </summary>
		IESafemode,

		/// <summary>
		/// 8以降のIE EnhancedProtectedMode
		/// </summary>
		IEEPMode,

		/// <summary>
		/// Firefox
		/// </summary>
		Firefox,

		/// <summary>
		/// PaleMoon
		/// </summary>
		PaleMoon,

		/// <summary>
		/// Songbird
		/// </summary>
		Songbird,

		/// <summary>
		/// SeaMonkey
		/// </summary>
		SeaMonkey,

        /// <summary>
		/// Google Chrome
		/// </summary>
		GoogleChrome,

		/// <summary>
		/// Comodo Dragon
		/// </summary>
		ComodoDragon,

		/// <summary>
		/// Comodo IceDragon
		/// </summary>
		ComodoIceDragon,

        /// <summary>
        /// Chrome Plus
        /// </summary>
        ChromePlus,
        
		/// <summary>
		/// CoolNovo
		/// </summary>
		CoolNovo,

		/// <summary>
		/// Opera
		/// </summary>
		OperaWebkit,

        /// <summary>
		/// Opera
		/// </summary>
		Opera,

		/// <summary>
		/// Opera
		/// </summary>
		Opera64,

        /// <summary>
		/// Safari
		/// </summary>
		Safari,

		/// <summary>
		/// Lunascape6 Geckoエンジン
		/// </summary>
		LunascapeGecko,

		/// <summary>
		/// Lunascape6 Webkitエンジン
		/// </summary>
		LunascapeWebkit,
		
		/// <summary>
		/// Sleipnir3 Geckoエンジン
		/// </summary>
		Sleipnir3Gecko,

		/// <summary>
		/// Sleipnir3 Webkitエンジン
		/// </summary>
		Sleipnir3Webkit,

		/// <summary>
		/// Sleipnir4 Blinkエンジン
		/// </summary>
		Sleipnir4Blink,

		/// <summary>
		/// RockMelt
		/// </summary>
		RockMelt,

		/// <summary>
		/// Maxthon
		/// </summary>
		Maxthon,

		/// <summary>
		/// Chromium
		/// </summary>
		Chromium,

		/// <summary>
		/// Sleipnir5 Blinkエンジン
		/// </summary>
		Sleipnir5Blink,

		/// <summary>
		/// Tungsten
		/// </summary>
		TungstenBlink
	}
}
