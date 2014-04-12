using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization;

namespace NwhoisLoginSystem
{
	class Utility
	{
		public static bool Serialize(string path, object graph)
		{

			try {
				IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
				using (Stream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None)) {
					formatter.Serialize(stream, graph);
					stream.Close();
				}
				
				return true;
			} catch {
			}

			return false;
		}

		public static object Deserialize(string path)
		{

			if (File.Exists(path)) {
				try {
					IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
					using (Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)) {
						return formatter.Deserialize(stream);
					}

				} catch {
				}

			}

			return null;
		}

		/// <summary>
		/// urlè„ÇÃÉyÅ[ÉWÇéÊìæÇ∑ÇÈ
		/// </summary>
		/// <param name="url"></param>
		/// <param name="cookies"></param>
		/// <param name="defaultTimeout"></param>
		/// <returns></returns>
		static public string GetResponseText(string url, CookieContainer cookies, int defaultTimeout)
		{
			HttpWebResponse webRes = null;
			StreamReader sr = null;

			try {
				HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(url);

				webReq.Timeout = defaultTimeout;
				webReq.CookieContainer = cookies;

				try {
					webRes = (HttpWebResponse)webReq.GetResponse();
				} catch (WebException ex) {
					webRes = ex.Response as HttpWebResponse;

				}

				if (webRes == null) {
					return null;
				}

				sr = new StreamReader(webRes.GetResponseStream(), System.Text.Encoding.UTF8);
				return sr.ReadToEnd();

			} finally {
				if (webRes != null)
					webRes.Close();
				if (sr != null)
					sr.Close();
			}
		}

		public static string GetUserName(Hal.CookieGetterSharp.ICookieGetter cookieGetter) {
			try {
				string url = "http://www.nicovideo.jp/my/channel";
				string name = "user_session";
				System.Net.CookieContainer container = new CookieContainer();
				container.Add(cookieGetter.GetCookie(new Uri(url), name));
				string res = GetResponseText(url, container,5000);

				if (!string.IsNullOrEmpty(res)) {

					System.Text.RegularExpressions.Match namem = System.Text.RegularExpressions.Regex.Match(res, "nickname = \"([^<>]+)\";", System.Text.RegularExpressions.RegexOptions.Singleline);
					if (namem.Success) {
						return namem.Groups[1].Value;
					}
				}

			} catch {
			}
			return null;
		}

	}
}
