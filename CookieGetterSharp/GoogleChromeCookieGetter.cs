using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Hal.CookieGetterSharp
{

	/// <summary>
	/// GoogleChromeからクッキーを取得する
	/// </summary>
	class GoogleChromeCookieGetter : SqlChromeCookieGetter
	{

		const string SELECT_QUERY = "SELECT value, name, host_key, path, expires_utc FROM cookies";

		public GoogleChromeCookieGetter(CookieStatus status) : base(status)
		{
		}

		protected override System.Net.Cookie DataToCookie(object[] data)
		{
			System.Net.Cookie cookie = new System.Net.Cookie();
			string value = data[0] as string;
			// なんというめちゃくちゃな
			// chrome cookie version 7
			if(data.Length == 6) {
				byte[] cipher = null;
				try {
					cipher = data[5] as byte[];
				}
				catch { }
				if(cipher == null || cipher.Length == 0) {
					cookie.Value = data[0] as string;
				}
				else {

					win32api.DATA_BLOB input;
					input.pbData = Marshal.AllocHGlobal(cipher.Length);
					input.cbData = (uint)cipher.Length;
					Marshal.Copy(cipher, 0, input.pbData, cipher.Length);
					win32api.DATA_BLOB output = new win32api.DATA_BLOB();
					win32api.DATA_BLOB dammy = new win32api.DATA_BLOB();
					if(win32api.CryptUnprotectData(ref input, null, ref dammy, IntPtr.Zero, IntPtr.Zero, 0, ref output)) {
						Debug.WriteLine(base.Status.DisplayName + ": CryptUnprotectData ok");
						byte[] plain = new byte[output.cbData];
						Marshal.Copy(output.pbData, plain, 0, (int)output.cbData);
						cookie.Value = Encoding.UTF8.GetString(plain);
						if(win32api.LocalFree(output.pbData) == IntPtr.Zero) {
							Debug.WriteLine(base.Status.DisplayName + ": output.pbData free");
						}
					}
					//Marshal.FreeHGlobal(input.pbData);
					if(win32api.LocalFree(input.pbData) == IntPtr.Zero) {
						Debug.WriteLine(base.Status.DisplayName + ": input.pbData free");
					}
				}
			}
			else {
				cookie.Value = data[0] as string;
			}

			cookie.Name = data[1] as string;
			cookie.Domain = data[2] as string;
			cookie.Path = data[3] as string;

			if (cookie.Value != null) {
				cookie.Value = Uri.EscapeDataString(cookie.Value);
			}

			try {
				long exp = long.Parse(data[4].ToString());
				// クッキー有効期限が正確に取得されていなかったので修正
				cookie.Expires = Utility.UnixTimeToDateTime((int)((long)(exp / 1000000) - 11644473600));
			} catch (Exception ex) {
				throw new CookieGetterException("GoogleChromeのexpires変換に失敗しました", ex);
			}

			return cookie;
		}

		protected override string MakeQuery()
		{
			return SELECT_QUERY + " ORDER BY creation_utc DESC";
		}

		protected override string MakeQuery(Uri url)
		{
			return string.Format("{0} {1} ORDER BY creation_utc DESC", SELECT_QUERY, makeWhere(url));
		}

		protected override string MakeQuery(Uri url, string key)
		{
			return string.Format("{0} {1} AND name = \"{2}\" ORDER BY creation_utc DESC", SELECT_QUERY, makeWhere(url), key);
		}
	}
}
