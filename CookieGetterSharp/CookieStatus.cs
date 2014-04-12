using System;
using System.Collections.Generic;
using System.Text;

namespace Hal.CookieGetterSharp
{
	/// <summary>
	/// PathがFileとDirectoryのどちらを示しているかを表す
	/// </summary>
	public enum PathType
	{

		/// <summary>
		/// ファイル
		/// </summary>
		File,

		/// <summary>
		/// ディレクトリ
		/// </summary>
		Directory
	}

	/// <summary>
	/// CookieGetterの状態を表すインターフェース
	/// </summary>
	[Serializable]
	public class CookieStatus
	{
		protected string _name;
		protected BrowserType _browserType;
		protected PathType _pathType;
		protected string _path;
		protected string _displayName;

		protected CookieStatus() { 
		}

		internal CookieStatus(string name, string path, BrowserType browserType, PathType pathType)
		{
			_name = name;
			_path = path;
			_browserType = browserType;
			_pathType = pathType;
			_displayName = null;
		}

		/// <summary>
		/// ブラウザの種類を取得する
		/// </summary>
		public BrowserType BrowserType
		{
			get { return _browserType; }
		}

		/// <summary>
		/// 利用可能かどうかを取得する
		/// </summary>
		public bool IsAvailable
		{
			get {
				if (string.IsNullOrEmpty(this.CookiePath)) return false;

				if (_pathType == PathType.File) {
					return System.IO.File.Exists(this.CookiePath);
				} else {
					return System.IO.Directory.Exists(this.CookiePath);
				}
			}
		}

		/// <summary>
		/// 識別名を取得する
		/// </summary>
		public string Name
		{
			get { return _name; }
			internal set { _name = value; }
		}

		/// <summary>
		/// クッキーが保存されているフォルダを取得、設定する
		/// </summary>
		public string CookiePath
		{
			get { return _path; }
			set { _path = value; }
		}

		/// <summary>
		/// CookiePathがFileを表すのか、Directoryを表すのかを取得する
		/// </summary>
		public PathType PathType {
			get { return _pathType; }
		}

		/// <summary>
		/// ToStringで表示される名前。nullにするとNameが表示されるようになる。
		/// </summary>
		public string DisplayName {
			get {
				if (string.IsNullOrEmpty(_displayName)) {
					return this.Name;
				} else {
					return _displayName;
				}
			}
			set {
				if (this.Name.Equals(value)) {
					_displayName = null;
				} else {
					_displayName = value;
				}
			}
		}

		#region Objectのオーバーライド

		/// <summary>
		/// DisplayNameを返します
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return this.DisplayName;
		}

		/// <summary>
		/// ブラウザ名、クッキー保存先が等しいかを調べます
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			if (this.Name == null || this.CookiePath == null || obj == null || !(obj is CookieStatus)) {
				return false;
			}
			CookieStatus bi = (CookieStatus)obj;

			return this.Name.Equals(bi.Name) && this.CookiePath.Equals(bi.CookiePath);
		}

		/// <summary>
		/// ハッシュコードを返します
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			string x = this.Name + this.CookiePath;
			return x.GetHashCode();
		}

		#endregion


	}
}
