using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace Hal.CookieGetterSharp {
	/// <summary>
	/// コンボボックスニコ生用ブラウザー
	/// </summary>
	public class ComboBoxNicoBrowser : ComboBox {
		private readonly BindingSource bindingSource = new BindingSource();
		private readonly List<ICookieGetter> cookieGetterList = new List<ICookieGetter>();
		private bool _allBrowser = false;
		private string _cookieValue = "";
		private string _cookiePath = "";
		private object browserLock = new object();
		private Thread browserThread = null;
		private bool browserCancel = false;
		private ManualResetEvent browserSignal = new ManualResetEvent(false);
		private int _ayncUpdateIndex = 0;
		
		/// <summary>
		/// ComboBoxNicoBrowserクラスの新しいインスタンスを既定のプロパティ値で初期化します。
		/// </summary>
		public ComboBoxNicoBrowser() : base() {
			CookieUrl = "http://www.nicovideo.jp/";
			CookieKey = "user_session";
			VerifyUrl = "http://www.nicovideo.jp/my/";
			VerifyRegex = "nickname = \"([^<>]+)\";";
			VerifyTimeout = 5000;

			DropDownStyle = ComboBoxStyle.DropDownList;
			Width = 240;

			browserThread = new Thread(() => {
				do {
					browserSignal.Reset();
					browserSignal.WaitOne();
					if(this.Disposing || this.IsDisposed) {
						break;
					}
					browserCancel = false;
					for(_ayncUpdateIndex = 0;_ayncUpdateIndex < this.Items.Count;_ayncUpdateIndex++) {
						lock(browserLock) {
							if(browserCancel) { break; }
							if(_ayncUpdateIndex < this.Items.Count) {
								ICookieGetter cookieGetter = this.Items[_ayncUpdateIndex] as ICookieGetter;
								if(cookieGetter != null && cookieGetter.Status.IsAvailable) {
									if(browserCancel) { break; }
									string name = GetUserName(cookieGetter);
									if(browserCancel) { break; }
									if(name != null) {
										cookieGetter.Status.DisplayName = string.Format("{0} ({1})", cookieGetter.Status.Name, name);
										BrowserChanged();
									}
								}
							}
						}
					}
				} while(!this.Disposing && !this.IsDisposed);
			});
			browserThread.SetApartmentState(ApartmentState.STA);
			browserThread.Name = "取得";
		}

		/// <summary>
		/// すべてのブラウザーを表示するかどうかを示します。
		/// </summary>
		[Category("動作")]
		[Description("すべてのブラウザーを表示するかどうかを示します。")]
		public bool AllBrowser {
			get {
				return _allBrowser;
			}
			set {
				if(_allBrowser != value) {
					_allBrowser = value;
					SetBrowser();
				}
			}
		}

		/// <summary>
		/// クッキーを取り出すUrlを取得、設定する
		/// </summary>
		[Category("動作")]
		[Description("クッキーを取り出すUrlを取得、設定します。")]
		[DefaultValue("http://www.nicovideo.jp/")]
		public string CookieUrl { get; set; }

		/// <summary>
		/// クッキーのKeyを取得、設定する
		/// </summary>
		[Category("動作")]
		[Description("クッキーのKeyを取得、設定します。")]
		[DefaultValue("user_session")]
		public string CookieKey { get; set; }

		/// <summary>
		/// クッキーのKeyに対するValueを取得する
		/// </summary>
		[System.ComponentModel.Browsable(false)]
		public string CookieValue {
			get {
				if(this.SelectedItem is ICookieGetter) {
					ICookieGetter cookieGetter = (ICookieGetter)this.SelectedItem;
					GetUserSession(cookieGetter);
				}
				return _cookieValue;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		[Category("動作")]
		[Description("")]
		[DefaultValue("http://www.nicovideo.jp/my/")]
		public string VerifyUrl { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[Category("動作")]
		[Description("")]
		[DefaultValue("nickname = \"([^<>]+)\";")]
		public string VerifyRegex { get; set; }

		/// <summary>
		/// タイムアウト値
		/// </summary>
		[Category("動作")]
		[Description("タイムアウト")]
		[DefaultValue("5000")]
		public int VerifyTimeout { get; set; }

		/// <summary>
		/// クッキーが保存されているフォルダを取得、設定する
		/// </summary>
		[System.ComponentModel.Browsable(false)]
		public string CookiePath {
			get {
				if(this.SelectedItem is ICookieGetter) {
					ICookieGetter cookieGetter = (ICookieGetter)this.SelectedItem;
					_cookiePath = cookieGetter.Status.CookiePath;
				}
				return _cookiePath;
			}
			set {
				if(this.SelectedItem is ICookieGetter) {
					ICookieGetter cookieGetter = (ICookieGetter)this.SelectedItem;
					cookieGetter.Status.CookiePath = value;
				}
			}
		}

		/// <summary>
		/// クッキーが保存されているフォルダを取得、設定する
		/// </summary>
		[System.ComponentModel.Browsable(false)]
		public string SelectedCookiePath {
			get {
				if(this.SelectedItem is ICookieGetter) {
					ICookieGetter cookieGetter = (ICookieGetter)this.SelectedItem;
					_cookiePath = cookieGetter.Status.CookiePath;
				}
				return _cookiePath;
			}
			set {
				if(this.SelectedItem is ICookieGetter) {
					ICookieGetter cookieGetter = (ICookieGetter)this.SelectedItem;
					cookieGetter.Status.CookiePath = value;
				}
			}
		}

		/// <summary>
		/// コンボボックスで選択されているブラウザのICookieGetterを返します
		/// ない場合nullを返す
		/// </summary>
		[System.ComponentModel.Browsable(false)]
		public ICookieGetter SelectedCookieGetter {
			get {
				return this.SelectedItem as ICookieGetter;
			}
		}

		/// <summary>
		/// CookieUrlとCookieKeyからコンボボックスで選択されているブラウザのCookieを返します
		/// </summary>
		[System.ComponentModel.Browsable(false)]
		public Cookie SelectedCookie {
			get {
				ICookieGetter cookieGetter = SelectedCookieGetter;
				return cookieGetter != null ? SelectedCookieGetter.GetCookie(new Uri(CookieUrl), CookieKey) : new Cookie();
			}
		}

		/// <summary>
		/// CookieGetterの状態を表す
		/// この値をバックアップ・リストアする
		/// </summary>
		[System.ComponentModel.Browsable(false)]
		public CookieStatus CookieStatus {
			get {
				if(this.SelectedItem is ICookieGetter) {
					ICookieGetter cookieGetter = (ICookieGetter)this.SelectedItem;
					cookieGetter.Status.CookiePath = this.GetCookiePath();
					return cookieGetter.Status;
				}
				return null;
			}

			set {

				if(value == null) {
					if(this.Items.Count != 0) {
						this.SelectedIndex = 0;
					}
				}
				else {

					foreach(ICookieGetter cookieGetter in cookieGetterList) {
						// すでにコンボボックスにある場合
						if(cookieGetter.Status.Equals(value)) {
							cookieGetter.Status.CookiePath = value.CookiePath;
							this.SelectedItem = cookieGetter;
							return;
						}
					}

					// コンボボックスにない場合
					ICookieGetter[] cookieGetters = CookieGetter.CreateInstances(false);
					ICookieGetter cookieGetterTemp = null;
					for(int i = 0; i < cookieGetters.Length; i++) {
						if(cookieGetters[i].Status.Name == value.Name) {
							if(File.Exists(value.CookiePath)) {
								cookieGetters[i].Status.CookiePath = value.CookiePath;
							}
							cookieGetterTemp = cookieGetters[i];
							break;
						}
					}
					if(cookieGetterTemp != null) {
						cookieGetterList.Add(cookieGetterTemp);
						this.bindingSource.Position = cookieGetterList.Count - 1;
					}
				}
			}
		}

		/// <summary>
		/// コンボボックスで選択されているブラウザのクッキーパスを返します
		/// ない場合nullを返す
		/// </summary>
		/// <returns></returns>
		public string GetBrowserName() {
			ICookieGetter cookieGetter = SelectedCookieGetter;
			return cookieGetter != null ? cookieGetter.Status.Name : null;
		}

		/// <summary>
		/// コンボボックスで選択されているブラウザのクッキーパスを返します
		/// ない場合nullを返す
		/// </summary>
		/// <returns></returns>
		public string GetCookiePath() {
			ICookieGetter cookieGetter = SelectedCookieGetter;
			return cookieGetter != null ? cookieGetter.Status.CookiePath : null;
		}

		/// <summary>
		/// コンボボックスで選択しているCookieGetterの状態をbase64にて取得する
		/// この値をSetCookieStatusに渡すことで状態を復元する
		/// 取得失敗時nullを返す
		/// </summary>
		/// <returns></returns>
		public string GetCookieStatus() {
			string cookieStatusBase64 = null;
			try {
				IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
				using(Stream stream = new MemoryStream()) {
					formatter.Serialize(stream, CookieStatus);
					byte[] bs = new byte[stream.Length];
					stream.Position = 0;
					stream.Read(bs, 0, (int)stream.Length);
					cookieStatusBase64 = System.Convert.ToBase64String(bs);
				}

			}
			catch {
			}
			return cookieStatusBase64;
		}

		/// <summary>
		/// GetCookieStatus()で取得した値を渡すことで状態を復元する
		/// </summary>
		/// <param name="cookieStatusBase64"></param>
		/// <returns></returns>
		public bool SetCookieStatus(string cookieStatusBase64) {
			try {
				byte[] bs = System.Convert.FromBase64String(cookieStatusBase64);
				IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
				using(Stream stream = new MemoryStream(bs.Length)) {
					stream.Position = 0;
					stream.Write(bs, 0, bs.Length);
					stream.Position = 0;
					CookieStatus = formatter.Deserialize(stream) as Hal.CookieGetterSharp.CookieStatus;
				}
				this.RefreshBrowser();
			}
			catch {
				return false;
			}

			return true;
		}

		/// <summary>
		/// GetBrowserName()で取得した値を渡すことで状態を復元する
		/// </summary>
		/// <param name="browserName">ICookieGetter.Status.Name</param>
		/// <returns></returns>
		public bool SetBrowser(string browserName) {
			return SetBrowser(browserName, null);
		}

		/// <summary>
		/// GetBrowserName()とGetCookiePath()で取得した値を渡すことで状態を復元する
		/// </summary>
		/// <param name="browserName">ICookieGetter.Status.Name</param>
		/// <param name="cookiePath">ICookieGetter.Status.CookiePath</param>
		/// <returns></returns>
		public bool SetBrowser(string browserName, string cookiePath) {
			for(int i = 0; i < cookieGetterList.Count; i++) {
				if(cookieGetterList[i].Status.Name == browserName) {
					if(!string.IsNullOrEmpty(cookiePath)) {
						if(cookieGetterList[i].Status.CookiePath != cookiePath) {
							ICookieGetter newCookieGetter = CookieGetter.CreateInstance(cookieGetterList[i].Status);
							if(newCookieGetter != null) {
								newCookieGetter.Status.CookiePath = cookiePath;
								cookieGetterList.Add(newCookieGetter);
								this.bindingSource.Position = cookieGetterList.Count - 1;
								return true;
							}
							else {
								return false;
							}
						}
					}
					bindingSource.Position = i;
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// 更新します
		/// </summary>
		public void RefreshBrowser() {
			SetBrowser();
		}



		#region 

		delegate void BrowserChangedCallback();
		private void BrowserChanged() {
			if(this.InvokeRequired) {
				BeginInvoke(new BrowserChangedCallback(BrowserChanged));
				return;
			}

			int selectedIndex = bindingSource.Position;
			ICookieGetter[] x = new ICookieGetter[cookieGetterList.Count];
			cookieGetterList.CopyTo(x, 0);
			cookieGetterList.Clear();
			cookieGetterList.AddRange(x);
			bindingSource.ResetBindings(false);
			this.bindingSource.Position = selectedIndex;
		}

		private void SetBrowser() {
			if(browserThread.ThreadState == ThreadState.Running) {
				browserCancel = true;
			}
			lock(browserLock) {
				ICookieGetter cookieGetter = null;
				if(this.SelectedItem is ICookieGetter) {
					cookieGetter = (ICookieGetter)this.SelectedItem;
				}

				cookieGetterList.Clear();
				cookieGetterList.AddRange(CookieGetter.CreateInstances(!_allBrowser));
				if(cookieGetter != null) {
					CookieStatus = cookieGetter.Status;
				}
				bindingSource.ResetBindings(false);
			}
			browserSignal.Set();
		}

		private string GetUserSession(ICookieGetter cookieGetter) {
			Cookie cookie = cookieGetter.GetCookie(new Uri(CookieUrl), CookieKey);

			if(cookie != null) {
				_cookieValue = cookie.Value;
			}
			else {
				_cookieValue = "";
			}

			return _cookieValue;
		}

		private string GetUserName(ICookieGetter cookieGetter) {
			try {
				System.Net.CookieContainer container = new CookieContainer();
				Cookie cookie = cookieGetter.GetCookie(new Uri(VerifyUrl), CookieKey);
				string res = null;
				if(cookie != null) {
					container.Add(cookie);
					res = Utility.GetResponseText(VerifyUrl, container, VerifyTimeout);
				}

				if(!string.IsNullOrEmpty(res)) {
					System.Text.RegularExpressions.Match namem = System.Text.RegularExpressions.Regex.Match(res, VerifyRegex, System.Text.RegularExpressions.RegexOptions.Singleline);
					if(namem.Success) {
						return namem.Groups[1].Value;
					}
				}

			}
			catch {
			}
			return null;
		}

		#endregion

		#region override

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		protected override ControlCollection CreateControlsInstance() {
			if(!this.DesignMode) {
				DataSource = bindingSource;
				bindingSource.DataSource = cookieGetterList;
				browserThread.Start();
				SetBrowser();
			}
			return base.CreateControlsInstance();
		}

		/// <summary>
		/// 項目を更新
		/// </summary>
		protected override void RefreshItems() {
			base.RefreshItems();
		}

		/// <summary>
		/// リソースの開放
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose(bool disposing) {
			browserSignal.Set();
			base.Dispose(disposing);
		}

		#endregion

	}
}
