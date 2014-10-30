using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace SunokoLibrary.Application
{
    /// <summary>
    /// ブラウザからのCookieを取得する機能を定義します。
    /// </summary>
    public interface ICookieImporter
    {
        BrowserConfig Config { get; }
        /// <summary>
        /// Cookie保存の形態を取得する
        /// </summary>
        PathType CookiePathType { get; }
        /// <summary>
        /// 利用可能かどうかを取得する
        /// </summary>
        bool IsAvailable { get; }
        /// <summary>
        /// 指定されたURLとの通信に使えるCookieを返します。
        /// </summary>
        /// <param name="targetUrl">通信先のURL</param>
        /// <param name="container">取得Cookieを入れる対象</param>
        /// <returns>処理の成功不成功</returns>
        Task<ImportResult> GetCookiesAsync(Uri targetUrl, CookieContainer container);
        /// <summary>
        /// 自身と設定の異なるICookieImporterを生成する
        /// </summary>
        ICookieImporter Generate(BrowserConfig config);
    }
    public enum PathType { File, Directory }
    /// <summary>
    /// Cookie取得の実行結果を定義します。
    /// </summary>
    public enum ImportResult
    {
        /// <summary>処理が正常終了状態にあります。</summary>
        Success,
        /// <summary>処理出来る状態下にありませんでした。</summary>
        Unavailable,
        /// <summary>データの参照に失敗。処理は中断されています。</summary>
        AccessError,
        /// <summary>データの解析に失敗。処理は中断されています。</summary>
        ConvertError,
        /// <summary>処理に失敗。想定されていないエラーが発生しています。</summary>
        UnknownError,
    }
    
    /// <summary>
    /// ブラウザに対して行える操作を定義します。
    /// </summary>
    public interface ICookieImporterFactory
    {
        /// <summary>
        /// 利用可能なすべてのCookieGetterを取得します
        /// </summary>
        /// <returns></returns>
        IEnumerable<ICookieImporter> CreateCookieImporters();
    }

    /// <summary>
    /// クッキー取得に関する例外
    /// </summary>
    [Serializable]
    public class CookieImportException : Exception
    {
        public CookieImportException(ImportResult result) { Result = result; }
        public CookieImportException(string message, ImportResult result)
            : base(message) { Result = result; }
        public CookieImportException(string message, ImportResult result, Exception inner)
            : base(message, inner) { Result = result; }

        public ImportResult Result { get; private set; }
    }
}