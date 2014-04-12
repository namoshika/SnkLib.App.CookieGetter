using System;
using System.Collections.Generic;
using System.Text;

namespace Hal.CookieGetterSharp
{
	class WebkitCookieGetter : CookieGetter
	{		

		public WebkitCookieGetter(CookieStatus status)
			: base(status)
		{ 
		}

		public override System.Net.CookieContainer GetAllCookies()
		{
			if (base.CookiePath == null || !System.IO.File.Exists(base.CookiePath)) {
				return new System.Net.CookieContainer();
				throw new CookieGetterException("Webkitのクッキーパスが正しく設定されていません。");
			}
			
			try {
				using(System.IO.StreamReader sr = new System.IO.StreamReader(this.Status.CookiePath)){
					while (!sr.EndOfStream) {
						string line = sr.ReadLine();
						if (line.StartsWith("cookies=")) {
							return ParseCookieSettings(line);
						}
					}
				}
			} catch (Exception ex){
				throw new CookieGetterException("Webkitのクッキー取得でエラーが発生しました。", ex);
			}

			throw new CookieGetterException("指定されたファイルにWebkit用のクッキー情報が含まれていませんでした。");
		}

		private System.Net.CookieContainer ParseCookieSettings(string line)
		{
			System.Net.CookieContainer container = new System.Net.CookieContainer();

			// クッキー情報の前についているよくわからないヘッダー情報を取り除く
			// 対象：
			// 　\\xと２桁の１６進数値
			// 　\\\\
			// 　\がない場合の先頭１文字
			string matchPattern = "^(\\\\x[0-9a-fA-F]{2})|^(\\\\\\\\)|^(.)|[\"()]";
			System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex(matchPattern, System.Text.RegularExpressions.RegexOptions.Compiled);

			string[] blocks = line.Split(new string[] { "\\0\\0\\0" }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string block in blocks) {
				if (block.Contains("=") && block.Contains("domain")) {
					string header = reg.Replace(block, "");
					System.Net.Cookie cookie = ParseCookie(header);
					if (cookie != null) {
						try {
							container.Add(cookie);
						} catch(Exception ex) {
							CookieGetter.Exceptions.Enqueue(ex);
						}
					}
				}
			}

			return container;
		}

		/// <summary>
		/// クッキーヘッダーをクッキーに変換する
		/// </summary>
		/// <param name="header"></param>
		/// <returns></returns>
		private System.Net.Cookie ParseCookie(string header) {
			if (string.IsNullOrEmpty(header)) {
				throw new ArgumentException("header");
			}
			System.Net.Cookie cookie = new System.Net.Cookie();
			bool isCookieHeader = false;

			foreach (string segment in header.Split(new string[]{";"}, StringSplitOptions.RemoveEmptyEntries)) {
				KeyValuePair<string, string> kvp = ParseKeyValuePair(segment.Trim());

				if (string.IsNullOrEmpty(kvp.Key)) {
					isCookieHeader = false;
					break;
				}

				switch (kvp.Key) { 
					case "domain":
						cookie.Domain = kvp.Value;
						isCookieHeader = true;
						break;
					case "expires":
						cookie.Expires = DateTime.Parse(kvp.Value);
						break;
					case "path":
						cookie.Path = kvp.Value;
						break;
					default:
						cookie.Name = kvp.Key;
						cookie.Value = kvp.Value;
						if (cookie.Value != null) {
							cookie.Value = Uri.EscapeDataString(cookie.Value);
						}
						break;
				}
			}
			if (isCookieHeader) {
				return cookie;
			} else {
				return null;
			}
		}

		private KeyValuePair<string, string> ParseKeyValuePair(string exp) {
			int eqindex = exp.IndexOf('=');
			if (eqindex != -1) {
				return new KeyValuePair<string, string>(exp.Substring(0, eqindex), exp.Substring(eqindex+1));
			}

			return new KeyValuePair<string, string>();
		}
	}
}
