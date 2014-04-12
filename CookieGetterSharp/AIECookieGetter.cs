using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;

namespace Hal.CookieGetterSharp {

	/// <summary>
	/// IEやトライデントエンジンを利用しているブラウザのクッキーを取得する
	/// </summary>
	abstract class AIECookieGetter : CookieGetter {
		protected bool _checkSubDirectory;

		public AIECookieGetter(CookieStatus status, bool checkSubDirectory)
			: base(status) {
			this._checkSubDirectory = false;
		}

		/// <summary>
		/// 対象URL上の名前がKeyであるクッキーを取得する
		/// </summary>
		/// <param name="url"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public override System.Net.Cookie GetCookie(Uri url, string key) {
			List<string> files = SelectFiles(url, GetAllFiles());
			List<System.Net.Cookie> cookies = new List<System.Net.Cookie>();
			foreach(string filepath in files) {
				System.Net.CookieContainer container = new System.Net.CookieContainer();

				foreach(System.Net.Cookie cookie in PickCookiesFromFile(filepath)) {
					if(cookie.Name.Equals(key)) {
						cookies.Add(cookie);
					}
				}
			}

			if(cookies.Count != 0) {
				// Expiresが最新のものを返す
				cookies.Sort(CompareCookieExpiresDesc);
				return cookies[0];
			}

			return null;
		}

		/// <summary>
		/// urlに関連付けられたクッキーを取得します。
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		public override System.Net.CookieCollection GetCookieCollection(Uri url) {
			//関係のあるファイルだけ調べることによってパフォーマンスを向上させる
			List<string> files = SelectFiles(url, GetAllFiles());

			List<System.Net.Cookie> cookies = new List<System.Net.Cookie>();
			foreach(string filepath in files) {
				cookies.AddRange(PickCookiesFromFile(filepath));
			}

			// Expiresが最新のもで上書きする
			cookies.Sort(CompareCookieExpiresAsc);
			System.Net.CookieCollection collection = new System.Net.CookieCollection();
			foreach(System.Net.Cookie cookie in cookies) {
				try {
					collection.Add(cookie);
				}
				catch(Exception ex) {
					CookieGetter.Exceptions.Enqueue(ex);
					System.Diagnostics.Debug.WriteLine(ex.Message);
				}
			}

			return collection;
		}

		public override System.Net.CookieContainer GetAllCookies() {

			List<System.Net.Cookie> cookies = new List<System.Net.Cookie>();

			foreach(string file in GetAllFiles()) {
				cookies.AddRange(PickCookiesFromFile(file));
			}

			// Expiresが最新のもで上書きする
			cookies.Sort(CompareCookieExpiresAsc);
			System.Net.CookieContainer container = new System.Net.CookieContainer();
			foreach(System.Net.Cookie cookie in cookies) {
				try {
					Utility.AddCookieToContainer(container, cookie);
				}
				catch(Exception ex) {
					CookieGetter.Exceptions.Enqueue(ex);
					System.Diagnostics.Debug.WriteLine(ex.Message);
				}
			}
			return container;
		}

		/// <summary>
		/// win32apiを使って対象URL上の名前がKeyであるクッキーを取得します
		/// </summary>
		/// <param name="url"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		protected abstract List<System.Net.Cookie> GetCookiesWinApi(Uri url, string key);

		/// <summary>
		/// urlで指定されたサイトで使用されるクッキーが保存されているファイルを選択する
		/// </summary>
		/// <param name="url"></param>
		/// <param name="files"></param>
		/// <returns></returns>
		protected virtual List<string> SelectFiles(Uri url, List<string> files) {
			List<string> results = new List<string>();
			// クッキーのファイル名はユーザー名+トップレベルドメインを除いたホスト名+識別番号となっている
			// けれどそんなことは知らないからURIからホスト名を引くことにするよ
			//string hostName = RemoveTopLevelDomain(url.Host);
			string hostName = url.Host;

			foreach(string filePath in files) {
				//	string fileName = System.IO.Path.GetFileNameWithoutExtension(filePath);
				string fileHostName = GetFileHostName(filePath);

				if(fileHostName != null && hostName.EndsWith(fileHostName)) {
					results.Add(filePath);
				}
			}

			return results;
		}

		/// <summary>
		/// すべてのクッキーファイルを取得してアカウントごとに配列化する
		/// </summary>
		/// <returns></returns>
		protected virtual List<string> GetAllFiles() {
			List<string> results = new List<string>();

			if(File.Exists(base.CookiePath)) {
				base.CookiePath = Path.GetDirectoryName(base.CookiePath);
			}
			if(base.CookiePath == null || !System.IO.Directory.Exists(base.CookiePath)) {
				throw new CookieGetterException("IEのクッキーパスが正しく設定されていません。");
			}

			Stack<string> cookieFolders = new Stack<string>();
			cookieFolders.Push(base.CookiePath);

			if(_checkSubDirectory) {
				// VISTA用などのLOWサブフォルダも検索範囲に含める
				System.IO.Directory.GetDirectories(base.CookiePath);
			}

			foreach(string folder in cookieFolders) {
				if(System.IO.Directory.Exists(folder)) {

					foreach(string filePath in System.IO.Directory.GetFiles(folder)) {
						//	if (filePath.Contains("@") && filePath.EndsWith(".txt")) {
						if(filePath.EndsWith(".txt")) {
							results.Add(filePath);
						}
					}
				}
			}

			return results;
		}

		/// <summary>
		/// 指定されたファイルからクッキーを取得する
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		protected virtual System.Net.Cookie[] PickCookiesFromFile(string filePath) {
			List<System.Net.Cookie> results = new List<System.Net.Cookie>();
			try {
				string data = System.IO.File.ReadAllText(filePath, Encoding.GetEncoding("Shift_JIS"));
				string[] blocks = data.Split('*');

				foreach(string block in blocks) {
					string[] lines = block.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

					if(7 < lines.Length) {
						System.Net.Cookie cookie = new System.Net.Cookie();
						Uri uri = new Uri("http://" + lines[2]);
						cookie.Name = lines[0];
						cookie.Value = lines[1];
						cookie.Domain = uri.Host;
						cookie.Path = uri.AbsolutePath;

						// ドメインの最初に.をつける
						cookie.Domain = AddDotDomain(cookie.Domain);

						// 有効期限を取得する
						long uexp = 0, lexp = 0;
						if(long.TryParse(lines[4], out lexp) && long.TryParse(lines[5], out uexp)) {
							DateTime dt = FileTimeToDateTime(lexp, uexp);
							/*
							if(dt > DateTime.Now.AddMonths(2)) {
								dt = DateTime.Now;
							}
							 */
							cookie.Expires = dt;
						}

						if(cookie.Expires < DateTime.Now.AddMonths(12)) {
							results.Add(cookie);
						}
					}

				}
			}
			catch(Exception ex) {
				throw new CookieGetterException("IEクッキーの解析に失敗しました。", ex);
			}

			return results.ToArray();
		}

		/// <summary>
		/// ドメインの最初に.をつける
		/// </summary>
		/// <param name="host"></param>
		/// <returns></returns>
		protected virtual string AddDotDomain(string host) {
			if(host.StartsWith("www.")) {
				host = host.TrimStart(new char[] { 'w' });
			}
			if(!host.StartsWith(".")) {
				host = '.' + host;
			}
			return host;
		}

		/// <summary>
		/// ホスト名からトップレベルドメインを取り除く
		/// </summary>
		/// <param name="host"></param>
		/// <returns></returns>
		protected virtual string RemoveTopLevelDomain(string host) {
			List<string> hosts = new List<string>(host.Split('.'));
			if(hosts.Count != 1) {
				hosts.RemoveAt(hosts.Count - 1);
			}
			return string.Join(".", hosts.ToArray());

		}

		/// <summary>
		/// ファイルの中身からホスト名を取得する
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		protected virtual string GetFileHostName(string fileName) {
			try {
				using(StreamReader sr = new StreamReader(fileName, Encoding.GetEncoding("Shift_JIS"))) {
					int count = 0;
					string line;
					while((line = sr.ReadLine()) != null) {
						count++;
						if(3 <= count) {
							try {
								Uri uri = new Uri(string.Format("http://{0}", line));
								return uri.Host;
							}
							catch { }
							// return RemoveTopLevelDomain(line);
						}
					}
				}
			}
			catch {
			}

			return null;

		}

		/// <summary>
		/// ファイルタイムを日付に直す
		/// http://wisdom.sakura.ne.jp/system/winapi/win32/win112.html
		/// </summary>
		/// <param name="low"></param>
		/// <param name="high"></param>
		/// <returns></returns>
		protected virtual DateTime FileTimeToDateTime(long low, long high) {
			long ticks = ((long)high << 32) + low;
			return new DateTime(ticks).AddYears(1600);
		}

		/// <summary>
		/// クッキーを有効期限の昇順に並び替える
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		protected static int CompareCookieExpiresAsc(System.Net.Cookie a, System.Net.Cookie b) {
			if(a == null && b == null) {
				return 0;
			}
			if(a == null) {
				return -1;
			}
			if(b == null) {
				return 1;
			}
			return a.Expires.CompareTo(b.Expires);
		}

		/// <summary>
		/// クッキーを有効期限の降順に並び替える
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		protected static int CompareCookieExpiresDesc(System.Net.Cookie a, System.Net.Cookie b) {
			if(a == null && b == null) {
				return 0;
			}
			if(a == null) {
				return -1;
			}
			if(b == null) {
				return 1;
			}
			return -a.Expires.CompareTo(b.Expires);
		}

	}
}

/*

* クッキーファイルの中身
* クッキーは * で区切られている
* 上から名前、値、URL、？、有効期限１、有効期限2、生成日１、生成日２となっている
* 日付はWindows32APIのFiletime
* クッキーの名前はユーザー名＠トップドメインを除いたホスト名[識別番号].txt

user_session
user_session_460838_-------------------
nicovideo.jp/
1536
423150080
30049020
4054468736
30042984
*
__utma
---------.---------.---------.---------.---------.1
nicovideo.jp/
1600
2350186496
32111674
2683696384
30043046
*
__utmz
8292653.--------.1.1.utmccn=(direct)|utmcsr=(direct)|utmcmd=(none)
nicovideo.jp/
1600
1543438336
30079759
2684186384
30043046
*



*/