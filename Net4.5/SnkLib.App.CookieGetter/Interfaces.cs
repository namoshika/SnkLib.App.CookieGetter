using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
#if !NET20
using System.Threading.Tasks;
using System.ServiceModel;
#endif

namespace SunokoLibrary.Application
{
    /// <summary>
    /// ブラウザからのCookieを取得する機能を定義します。
    /// </summary>
    public interface ICookieImporter
    {
        /// <summary>
        /// Cookieを取得するブラウザに関する情報を取得します。
        /// </summary>
        CookieSourceInfo SourceInfo { get; }
        /// <summary>
        /// Cookie保存の形態を取得します。
        /// </summary>
        CookiePathType CookiePathType { get; }
        /// <summary>
        /// 利用可能かどうかを取得します。
        /// </summary>
        bool IsAvailable { get; }
        /// <summary>
        /// 並べ替え時に用いられる数値を取得します。OSブラウザ: 0、有名ブラウザ: 1、派生ブラウザ: 2。
        /// </summary>
        int PrimaryLevel { get; }
        /// <summary>
        /// 自身と設定の異なるICookieImporterを生成します。
        /// </summary>
        ICookieImporter Generate(CookieSourceInfo newInfo);
        /// <summary>
        /// 指定されたURLとの通信に使えるCookieを返します。
        /// </summary>
        /// <param name="targetUrl">通信先のURL</param>
        /// <returns>処理の成功不成功</returns>
        Task<CookieImportResult> GetCookiesAsync(Uri targetUrl);
    }
    /// <summary>
    /// Cookie取得結果を扱うクラスです。
    /// </summary>
    public struct CookieImportResult
    {
#pragma warning disable 1591
        public CookieImportResult(CookieCollection cookies, CookieImportState status)
        {
            _cookies = cookies;
            _status = status;
        }
#pragma warning restore 1591

        CookieCollection _cookies;
        CookieImportState _status;

        /// <summary>
        /// ブラウザから取得されたCookieを取得します。
        /// </summary>
        public CookieCollection Cookies { get { return _cookies; } }
        /// <summary>
        /// 処理の成功不成功の状態を取得します。
        /// </summary>
        public CookieImportState Status { get { return _status; } }
        /// <summary>
        /// 引数として指定したCookieContainerにブラウザから取得したCookieを追加します。
        /// </summary>
        /// <param name="targetContainer">追加先のコンテナ</param>
        /// <returns>インスタンスが保持するStatusをそのまま返します。</returns>
        public CookieImportState AddTo(CookieContainer targetContainer)
        {
            if (Status == CookieImportState.Success)
                targetContainer.Add(Cookies);
            return Status;
        }
    }
    /// <summary>
    /// パス指定対象の種類を定義します。
    /// </summary>
    public enum CookiePathType
    {
        /// <summary>ファイル</summary>
        File,
        /// <summary>フォルダ</summary>
        Directory,
    }
    /// <summary>
    /// Cookie取得の実行結果を定義します。
    /// </summary>
    public enum CookieImportState
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
        /// 対応しているブラウザエンジンの識別子の配列を取得します。
        /// </summary>
        string[] EngineIds { get; }
        /// <summary>
        /// 利用可能なすべてのICookieImporterを取得します。
        /// </summary>
        IEnumerable<ICookieImporter> GetCookieImporters();
        /// <summary>
        /// 指定されたブラウザ構成情報からICookieImporterを取得します。
        /// </summary>
        /// <param name="sourceInfo">元となるブラウザ構成情報。</param>
        /// <returns>引数で指定されたブラウザを参照するインスタンス。</returns>
        ICookieImporter GetCookieImporter(CookieSourceInfo sourceInfo);
    }
    /// <summary>
    /// 使用可能なICookieImporterの管理を行う機能を定義します。
    /// </summary>
    public interface ICookieImporterManager
    {
        /// <summary>
        /// 使用できるICookieImporterのリストを取得します。
        /// </summary>
        /// <param name="availableOnly">利用可能なものに絞る</param>
        Task<ICookieImporter[]> GetInstancesAsync(bool availableOnly);

        /// <summary>
        /// 設定値を指定したICookieImporterを取得します。アプリ終了時に直前まで使用していた
        /// ICookieImporterのSourceInfoを設定として保存すれば、起動時にSourceInfoをこのメソッドに
        /// 渡す事で適切なICookieImporterを再生成してくれる。
        /// </summary>
        /// <param name="targetSourceInfo">再取得対象のブラウザの構成情報</param>
        /// <param name="allowDefault">取得不可の場合に既定のCookieImporterを返すかを指定できます。</param>
        Task<ICookieImporter> GetInstanceAsync(CookieSourceInfo targetSourceInfo, bool allowDefault);
    }

    /// <summary>
    /// Cookie取得に関する例外。
    /// </summary>
    [Serializable]
    public class CookieImportException : Exception
    {
        /// <summary>例外を生成します。</summary>
        /// <param name="message">エラーの捕捉</param>
        /// <param name="result">エラーの種類</param>
        public CookieImportException(string message, CookieImportState result)
            : base(message) { Result = result; }
        /// <summary>例外を再スローさせるための例外を生成します。</summary>
        /// <param name="message">エラーの捕捉</param>
        /// <param name="result">エラーの種類</param>
        /// <param name="inner">内部例外</param>
        public CookieImportException(string message, CookieImportState result, Exception inner)
            : base(message, inner) { Result = result; }

        /// <summary>
        /// 例外要因の大まかな種類
        /// </summary>
        public CookieImportState Result { get; private set; }
    }
}