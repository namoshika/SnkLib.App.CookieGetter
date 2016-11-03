using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace SunokoLibrary.Application.Browsers
{
    /// <summary>
    /// IEのCacheファイルから直接Cookieを取得します。
    /// </summary>
    public class IEFindCacheCookieImporter : CookieImporterBase
    {
#pragma warning disable 1591
        //クラス命名センスとしてwininet.dllのFindNextUrlCacheEntryの文脈を用いる。
        //純粋にapi上で片付ける方法が不明なのでwininetのapi自体は使っていない。
        public IEFindCacheCookieImporter(CookieSourceInfo info, int primaryLevel)
            : base(info, CookiePathType.Directory, primaryLevel) { }
        public override bool IsAvailable
        {
            get
            {
                return string.IsNullOrEmpty(SourceInfo.CookiePath)
                    ? false : System.IO.Directory.Exists(SourceInfo.CookiePath);
            }
        }
        public override ICookieImporter Generate(CookieSourceInfo newInfo)
        { return new IEFindCacheCookieImporter(newInfo, PrimaryLevel); }
        protected override CookieImportResult ProtectedGetCookies(Uri targetUrl)
        {
            if (IsAvailable == false)
                return new CookieImportResult(null, CookieImportState.Unavailable);

            List<Cookie> cookieList;
            try
            {
                //関係のあるファイルだけ調べることによってパフォーマンスを向上させる
                cookieList = 
                    Enumerable.Concat(
                        Directory.EnumerateFiles(SourceInfo.CookiePath, "*.txt"),
                        Directory.EnumerateFiles(SourceInfo.CookiePath, "*.cookie"))
                    .Select(filePath => ReadAllTextIfHasSendableCookie(filePath, targetUrl))
                    .Where(data => string.IsNullOrEmpty(data) == false)
                    .SelectMany(data => ParseCookies(data))
                    .ToList();
            }
            catch (CookieImportException ex)
            {
                TraceError(this, "Cookie読み込みに失敗。", ex.ToString());
                return new CookieImportResult(null, ex.Result);
            }
            catch (IOException ex)
            {
                TraceError(this, "Cookie読み込みに失敗。", ex.ToString());
                return new CookieImportResult(null, CookieImportState.AccessError);
            }

            //Cookieを有効期限で昇順に並び替えて、Expiresが最新のもので上書きされるようにする
            cookieList.Sort((a, b) =>
                a == null && b == null ? 0 :
                a == null ? -1 :
                b == null ? 1 :
                a.Expires.CompareTo(b.Expires));

            var cookies = new CookieCollection();
            foreach (var cookie in cookieList)
                cookies.Add(cookie);
            return new CookieImportResult(cookies, CookieImportState.Success);
        }
#pragma warning restore 1591

        /// <summary>
        /// IEのCookieテキストからCookieを取得します。
        /// </summary>
        /// <exception cref="CookieImportException" />
        IEnumerable<Cookie> ParseCookies(string cacheCookiesText)
        {
            var cookies = new List<Cookie>();
            var blocks = cacheCookiesText.Split(new string[] { "*\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var block in blocks)
            {
                var lines = block.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                if (7 < lines.Length)
                {
                    var cookie = new Cookie();
                    var uri = new Uri("http://" + lines[2]);
                    cookie.Name = lines[0];
                    cookie.Value = lines[1];
                    cookie.Path = uri.AbsolutePath;
                    // ドメインの最初に.をつける
                    var domain = uri.Host;
                    if (domain.StartsWith("www."))
                        domain = domain.TrimStart(new char[] { 'w' });
                    if (domain.StartsWith(".") == false)
                        domain = '.' + domain;
                    cookie.Domain = domain;

                    // 有効期限を取得する
                    long uexp, lexp;
                    if (long.TryParse(lines[5], out uexp) == false || long.TryParse(lines[4], out lexp) == false)
                        throw new CookieImportException("キャッシュCookieの解析に失敗しました。", CookieImportState.ConvertError);
                    var ticks = ((long)uexp << 32) + lexp;
                    cookie.Expires = DateTime.FromFileTimeUtc(ticks);
                    cookies.Add(cookie);
                }
                else
                    throw new CookieImportException("キャッシュCookieの解析に失敗しました。", CookieImportState.ConvertError);
            }
            return cookies;
        }
        /// <summary>
        /// IEのCookieファイルを読み込みます。この時、引数sendingTargetへ送信できる
        /// Cookieが含まれるファイルのみが読み込まれます。
        /// </summary>
        /// <param name="cacheFilePath">Cookieファイル</param>
        /// <param name="sendingTarget">通信したいURL</param>
        /// <returns>Cookieファイル本文。</returns>
        /// <exception cref="CookieImportException" />
        /// <exception cref="OutOfMemoryException" />
        static string ReadAllTextIfHasSendableCookie(string cacheFilePath, Uri sendingTarget)
        {
            Exception ex = null;
            Uri domainAndPath = null;
            try
            {
                using (var sr = new StreamReader(cacheFilePath, Encoding.GetEncoding("Shift_JIS")))
                {
                    string line;
                    var builder = new StringBuilder();
                    for (var lineIdx = 0; (line = sr.ReadLine()) != null; lineIdx++)
                    {
                        builder.AppendLine(line);
                        if (lineIdx >= 2 && Uri.TryCreate(string.Format("http://{0}", line), UriKind.Absolute, out domainAndPath))
                            break;
                    }
                    if (domainAndPath != null && sendingTarget.Host.EndsWith(domainAndPath.Host))
                    {
                        builder.Append(sr.ReadToEnd().Replace("\n", "\r\n"));
                        return builder.ToString();
                    }
                    else
                        return null;
                }
            }
            catch (OutOfMemoryException) { throw; }
            catch (IOException e) { ex = e; }
            catch (System.Security.SecurityException e) { ex = e; }
            throw new CookieImportException("キャッシュCookieの読み込みに失敗。", CookieImportState.AccessError, ex);
        }
    }
}