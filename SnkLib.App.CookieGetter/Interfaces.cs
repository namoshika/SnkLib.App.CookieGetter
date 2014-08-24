using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

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
        bool GetCookies(Uri targetUrl, CookieContainer container);
        /// <summary>
        /// 自身と設定の異なるICookieImporterを生成する
        /// </summary>
        ICookieImporter Generate(BrowserConfig config);
    }
    public enum PathType { File, Directory }
    
    /// <summary>
    /// ブラウザに対して行える操作を定義します。
    /// </summary>
    public interface ICookieImporterFactory
    {
        /// <summary>
        /// 利用可能なすべてのCookieGetterを取得します
        /// </summary>
        /// <returns></returns>
        ICookieImporter[] CreateCookieImporters();
    }

    /// <summary>
    /// クッキー取得に関する例外
    /// </summary>
    [global::System.Serializable]
    public class CookieImportException : Exception
    {
        /// <summary>
        /// クラスの新しいインスタンスを初期化します。
        /// </summary>
        public CookieImportException() { }

        /// <summary>
        /// 指定したエラー メッセージを使用して、System.Exception クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="message">エラーを説明するメッセージ。</param>
        public CookieImportException(string message) : base(message) { }

        /// <summary>
        /// 指定したエラー メッセージと、この例外の原因である内部例外への参照を使用して、System.Exception クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="message">例外の原因を説明するエラー メッセージ。</param>
        /// <param name="inner">現在の例外の原因である例外。内部例外が指定されていない場合は、null 参照 (Visual Basic の場合は Nothing)。</param>
        public CookieImportException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        /// シリアル化したデータを使用して、System.Exception クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="info">スローされている例外に関するシリアル化済みオブジェクト データを保持している System.Runtime.Serialization.SerializationInfo。</param>
        /// <param name="context">転送元または転送先に関するコンテキスト情報を含んでいる System.Runtime.Serialization.StreamingContext。</param>
        protected CookieImportException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}