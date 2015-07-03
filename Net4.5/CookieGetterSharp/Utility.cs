using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;

namespace Hal.CookieGetterSharp
{
    /// <summary>
    /// 小規模関数群
    /// </summary>
    public class Utility
    {
        /// <summary>
        /// Unix時間をDateTimeに変換する
        /// </summary>
        /// <param name="UnixTime"></param>
        /// <returns></returns>
        public static DateTime UnixTimeToDateTime(int UnixTime)
        {
            return new DateTime(1970, 1, 1, 9, 0, 0).AddSeconds(UnixTime);
        }

        /// <summary>
        /// DateTimeをUnix時間に変換する
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static int DateTimeToUnixTime(DateTime time)
        {
            TimeSpan t = time.Subtract(new DateTime(1970, 1, 1, 9, 0, 0));
            return (int)t.TotalSeconds;
        }

        /// <summary>
        /// %APPDATA%などを実際のパスに変換する
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string ReplacePathSymbols(string path)
        {
            path = path.Replace("%APPDATA%", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
            path = path.Replace("%LOCALAPPDATA%", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
            path = path.Replace("%COOKIES%", Environment.GetFolderPath(Environment.SpecialFolder.Cookies));
            return path;
        }

        /// <summary>
        /// 必要があればuriの最後に/をつける
        /// Pathの指定がある場合、uriの最後に/があるかないかで取得できない場合があるので
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static Uri AddSrashLast(Uri uri)
        {
            string o = uri.Segments[uri.Segments.Length - 1];
            string no = uri.OriginalString;//.Replace("http://", "http://o.");
            if (!o.Contains(".") && o[o.Length - 1] != '/')
            {
                no += "/";
            }
            return new Uri(no);
        }

        /// <summary>
        /// クッキーコンテナにクッキーを追加する
        /// domainが.hal.fscs.jpなどだと http://hal.fscs.jp でクッキーが有効にならないので.ありとなし両方指定する
        /// </summary>
        /// <param name="container"></param>
        /// <param name="cookie"></param>
        public static void AddCookieToContainer(System.Net.CookieContainer container, System.Net.Cookie cookie)
        {

            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            if (cookie == null)
            {
                throw new ArgumentNullException("cookie");
            }

            container.Add(cookie);
            if (cookie.Domain.StartsWith("."))
            {
                container.Add(new System.Net.Cookie()
                {
                    Comment = cookie.Comment,
                    CommentUri = cookie.CommentUri,
                    Discard = cookie.Discard,
                    Domain = cookie.Domain.Substring(1),
                    Expired = cookie.Expired,
                    Expires = cookie.Expires,
                    HttpOnly = cookie.HttpOnly,
                    Name = cookie.Name,
                    Path = cookie.Path,
                    Port = cookie.Port,
                    Secure = cookie.Secure,
                    Value = cookie.Value,
                    Version = cookie.Version,
                });
            }

        }

        /// <summary>
        /// 実行プログラムフォルダ内に一時ファイル名の取得
        /// </summary>
        /// <returns></returns>
        public static string GetTempFilePath()
        {
            string uniqueString = Guid.NewGuid().ToString() + (System.Environment.TickCount & int.MaxValue).ToString();
            //	string applicationPath = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
            string roamingPath = System.IO.Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CookieGetterSharp");
            if (!System.IO.Directory.Exists(roamingPath)) System.IO.Directory.CreateDirectory(roamingPath);
            string uniquePath = System.IO.Path.Combine(roamingPath, md5(uniqueString));
            return uniquePath;
        }

        /// <summary>
        /// MD5ハッシュを生成
        /// </summary>
        /// <param name="textData"></param>
        /// <returns></returns>
        public static string md5(string textData)
        {
            byte[] byteData = System.Text.Encoding.UTF8.GetBytes(textData);
            System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] bs = md5.ComputeHash(byteData);
            System.Text.StringBuilder result = new System.Text.StringBuilder();
            foreach (byte b in bs)
            {
                result.Append(b.ToString("x2"));
            }

            return result.ToString();
        }

        /// <summary>
        /// url上のページを取得する
        /// </summary>
        /// <param name="url"></param>
        /// <param name="cookies"></param>
        /// <param name="defaultTimeout"></param>
        /// <returns></returns>
        static internal string GetResponseText(string url, CookieContainer cookies, int defaultTimeout)
        {
            HttpWebResponse webRes = null;
            StreamReader sr = null;

            try
            {
                HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(url);
                webReq.Timeout = defaultTimeout;
                webReq.CookieContainer = cookies;

                try
                {
                    webRes = (HttpWebResponse)webReq.GetResponse();
                }
                catch (WebException ex)
                {
                    webRes = ex.Response as HttpWebResponse;

                }

                if (webRes == null)
                {
                    return null;
                }

                sr = new StreamReader(webRes.GetResponseStream(), System.Text.Encoding.UTF8);
                return sr.ReadToEnd();

            }
            finally
            {
                if (webRes != null)
                    webRes.Close();
                if (sr != null)
                    sr.Close();
            }
        }

        /// <summary>
        /// シリアライズします
        /// </summary>
        /// <param name="path"></param>
        /// <param name="graph"></param>
        /// <returns></returns>
        public static bool Serialize(string path, object graph)
        {

            try
            {

                System.Runtime.Serialization.IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                using (Stream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    formatter.Serialize(stream, graph);
                    stream.Close();
                }

                return true;
            }
            catch
            {
            }

            return false;
        }

        /// <summary>
        /// デシリアライズします
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static object Deserialize(string path)
        {

            if (File.Exists(path))
            {
                try
                {
                    System.Runtime.Serialization.IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    using (Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        return formatter.Deserialize(stream);
                    }

                }
                catch
                {
                }

            }

            return null;
        }

    }
}