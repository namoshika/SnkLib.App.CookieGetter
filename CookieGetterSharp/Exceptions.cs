using System;
using System.Collections.Generic;
using System.Text;

namespace Hal.CookieGetterSharp
{

	/// <summary>
	/// クッキー取得に関する例外
	/// </summary>
	[global::System.Serializable]
	public class CookieGetterException : Exception
	{
		/// <summary>
		/// クラスの新しいインスタンスを初期化します。
		/// </summary>
		public CookieGetterException() { }

		/// <summary>
		/// 指定したエラー メッセージを使用して、System.Exception クラスの新しいインスタンスを初期化します。
		/// </summary>
		/// <param name="message">エラーを説明するメッセージ。</param>
		public CookieGetterException(string message) : base(message) { }

		/// <summary>
		/// 指定したエラー メッセージと、この例外の原因である内部例外への参照を使用して、System.Exception クラスの新しいインスタンスを初期化します。
		/// </summary>
		/// <param name="message">例外の原因を説明するエラー メッセージ。</param>
		/// <param name="inner">現在の例外の原因である例外。内部例外が指定されていない場合は、null 参照 (Visual Basic の場合は Nothing)。</param>
		public CookieGetterException(string message, Exception inner) : base(message, inner) { }

		/// <summary>
		/// シリアル化したデータを使用して、System.Exception クラスの新しいインスタンスを初期化します。
		/// </summary>
		/// <param name="info">スローされている例外に関するシリアル化済みオブジェクト データを保持している System.Runtime.Serialization.SerializationInfo。</param>
		/// <param name="context">転送元または転送先に関するコンテキスト情報を含んでいる System.Runtime.Serialization.StreamingContext。</param>
		protected CookieGetterException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
	}


}
