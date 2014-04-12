using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Hal.CookieGetterSharp
{
	class OperaCookieGetter : CookieGetter
	{
		const byte MSB = 0x80;

		private struct Header
		{
			public int file_version_number;
			public int app_version_number;
			public int idtag_length;
			public int length_length;
		}

		private struct Record
		{
			// application specific tag to identify content type
			public int tag_id;
			// length of payload
			public int length;
			// Payload/content of the record
			public byte[] bytepayload;
		};

		public OperaCookieGetter(CookieStatus status) : base(status)
		{
		}

		
		public override System.Net.Cookie GetCookie(Uri url, string key) {
			// 無理矢理取得 2011-11-19
			if(url.OriginalString.Contains(".nicovideo.jp") && key == "user_session") {
				System.Net.Cookie cookie = GetFileToNicoCookie(url, key);
				if(cookie != null) {
					return cookie;
				}
			}

			System.Net.CookieCollection collection = GetCookieCollection(url);
			return collection[key];
		}

		private System.Net.Cookie GetFileToNicoCookie(Uri url, string key) {
			// いつも通りへぼいソース 2011-11-19
			// へぼい内容→バイナリを無理矢理文字列にしてから正規表現でさくっと
			if(base.CookiePath == null || !File.Exists(base.CookiePath)) {
				return null;
			}
			
			System.Net.Cookie cookie = new System.Net.Cookie();

			try {
				byte[] CookieData = System.IO.File.ReadAllBytes(base.CookiePath);
				String CookieTxt = Encoding.ASCII.GetString(CookieData);
				CookieTxt = CookieTxt.Replace("\0", "\t");
				string regexText = string.Format("{0}_([0-9]+)_([0-9]+)", key);
				Match cookieChunk = Regex.Match(CookieTxt, @regexText, RegexOptions.Multiline);
				if(cookieChunk.Success) {
					try {
						cookie.Name = key;
						cookie.Value = string.Format("{0}_{1}_{2}", key, cookieChunk.Groups[1].Value, cookieChunk.Groups[2].Value);
						cookie.Domain = ".nicovideo.jp";	// 2011-11-19fix1
						cookie.Path = "/";
					}
					catch(Exception ex) {
						CookieGetter.Exceptions.Enqueue(ex);
						Debug.WriteLine(string.Format("Invalid Format! domain:{0},key:{1},value:{2}", cookie.Domain, cookie.Name, cookie.Value));
					}
				}
				else {
					cookie = null;
				}

			}
			catch(Exception ex) {
				throw new CookieGetterException("Operaのクッキー取得中にエラーが発生しました。", ex);
			}

			return cookie;
		}
		

		public override System.Net.CookieContainer GetAllCookies()
		{

			System.Net.CookieContainer container = new System.Net.CookieContainer();

			if (base.CookiePath == null || !File.Exists(base.CookiePath)) {
				return container;
				throw new CookieGetterException("Operaのクッキーパスが正しく設定されていません。");
			}
			
			

			try {
				using (FileStream reader = new FileStream(base.CookiePath, FileMode.Open, FileAccess.Read)) {
					// 指定したアドレスに読み込み位置を移動
					reader.Seek(0, SeekOrigin.Begin);
					Header headerData = getHeader(reader);
					Record recordData;
					Stack<string> domainStack = new Stack<string>();
					Stack<string> pathStack = new Stack<string>();

					//version check
					if ((headerData.file_version_number & 0xfffff000) == 0x00001000) {

						while (reader.Position < reader.Length) {
							recordData = getRecord(reader, headerData);
							switch (recordData.tag_id) {
								case 0x01:  // ドメイン
									string domain;
									using(System.IO.MemoryStream ms = new System.IO.MemoryStream(recordData.bytepayload)){
										domain = getDomainRecode(ms, headerData);
									}
									if (domain != null) {
										domainStack.Push(domain);
									}
									break;
								case 0x02:  // パス
									string page;
									using (System.IO.MemoryStream ms = new System.IO.MemoryStream(recordData.bytepayload)) {
										page = getPageRecode(ms, headerData);
									}

									if (page != null) {
										pathStack.Push(page);
									}
									break;
								case 0x03:  // クッキー
									string chost = string.Join(".", domainStack.ToArray());
									string cpath = '/' + string.Join("/", pathStack.ToArray());

									System.Net.Cookie cookie;
									using (System.IO.MemoryStream ms = new System.IO.MemoryStream(recordData.bytepayload)) {
										cookie = getCookieRecode(ms, headerData);
									}
									cookie.Domain = '.' + string.Join(".", domainStack.ToArray());
									cookie.Path = '/' + string.Join("/", pathStack.ToArray());
									try {
										Utility.AddCookieToContainer(container, cookie);
									} catch (Exception ex){
										CookieGetter.Exceptions.Enqueue(ex);
										Console.WriteLine(string.Format("Invalid Format! domain:{0},key:{1},value:{2}", cookie.Domain, cookie.Name, cookie.Value));
									}

									break;
								case 0x04 + MSB: //ドメイン終了
									if (0 < domainStack.Count) {
										domainStack.Pop();
									}
									break;
								case 0x05 + MSB: //パス終了
									if (0 < pathStack.Count) {
										pathStack.Pop();
									}
									break;
							}
						}
					}
				}
			} catch (Exception ex) {
				throw new CookieGetterException("Operaのクッキー取得でエラーが発生しました。", ex);
			}

			return container;
		}

		private string getDomainRecode(System.IO.Stream stream, Header headerData)
		{
			Record recordData;

			while (stream.Position < stream.Length) {
				recordData = getRecord(stream, headerData);

				switch (recordData.tag_id) {
					case 0x1e:  // Domain Name
						return Encoding.ASCII.GetString(recordData.bytepayload);
				}
			}

			return null;
		}

		private string getPageRecode(System.IO.Stream stream, Header headerData)
		{
			Record recordData;

			while (stream.Position < stream.Length) {
				recordData = getRecord(stream, headerData);

				switch (recordData.tag_id) {
					case 0x1d:  // Page Name
						return Encoding.ASCII.GetString(recordData.bytepayload);
				}
			}

			return null;
		}

		private System.Net.Cookie getCookieRecode(System.IO.Stream stream, Header headerData)
		{
			Record recordData;
			System.Net.Cookie cookie = new System.Net.Cookie();

			while (stream.Position < stream.Length) {

				recordData = getRecord(stream, headerData);

				switch (recordData.tag_id) {
					case 0x10:  // Cookie Name
						cookie.Name = Encoding.ASCII.GetString(recordData.bytepayload);
						break;
					case 0x11:  // Cookie Value
						cookie.Value = Encoding.ASCII.GetString(recordData.bytepayload);
						if (cookie.Value != null) {
							cookie.Value = Uri.EscapeDataString(cookie.Value);
						}
						break;
					case 0x12:
						long time;
						using (System.IO.MemoryStream ms = new System.IO.MemoryStream(recordData.bytepayload)) {
							time = getNumber(ms, 8);
						}
						cookie.Expires = Utility.UnixTimeToDateTime((int)time);
						break;
				}
			}

			return cookie;
		}

		private Header getHeader(System.IO.Stream stream)
		{
			Header headerData = new Header();

			headerData.file_version_number = (int)getNumber(stream, 4);
			headerData.app_version_number = (int)getNumber(stream, 4);
			headerData.idtag_length = (int)getNumber(stream, 2);
			headerData.length_length = (int)getNumber(stream, 2);

			return headerData;
		}

		private Record getRecord(System.IO.Stream stream, Header headerData)
		{
			Record recordData = new Record();
			int topData = stream.ReadByte();
			stream.Seek(-1, SeekOrigin.Current);
			recordData.tag_id = (int)getNumber(stream, headerData.idtag_length);

			// MSBがONのとき、Tag IDのみになる。（以降のDataLength,Dataはない）
			if ((topData & MSB) == 0) {

				recordData.length = (int)getNumber(stream, headerData.length_length);
				recordData.bytepayload = new byte[recordData.length];
				stream.Read(recordData.bytepayload, 0, recordData.length);
			}

			return recordData;
		}

		private long getNumber(System.IO.Stream stream, int length)
		{
			long n = 0;
			for (int i = 0; i < length; i++) {
				n <<= 8;
				n += stream.ReadByte();
			}
			return n;
		}

	}
}
