using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Hal.CookieGetterSharp;

namespace NwhoisLoginSystem
{
	public partial class Form1 : Form
	{
		const string SETTING_FILE = "NwhoisLoginSystem.bin";

		int _ayncUpdateIndex = 0;
		object _asyncObj = new object();

		public Form1()
		{
			InitializeComponent();
			cmbBrowser.Items.AddRange(CookieGetter.CreateInstances(true));
		}

		public CookieStatus CookieStatus
		{
			get
			{
				if (cmbBrowser.SelectedItem is ICookieGetter) {
					ICookieGetter cookieGetter = (ICookieGetter)cmbBrowser.SelectedItem;
					cookieGetter.Status.CookiePath = txtPath.Text;
					return cookieGetter.Status;
				}
				return null;
			}

			set
			{

				if (value == null) {
					if (cmbBrowser.Items.Count != 0) {
						cmbBrowser.SelectedIndex = 0;
					}
				} else {

					foreach (ICookieGetter cookieGetter in cmbBrowser.Items) {
						// すでにコンボボックスにある場合
						if (cookieGetter.Status.Equals(value)) {
							cookieGetter.Status.CookiePath = value.CookiePath;
							cmbBrowser.SelectedItem = cookieGetter;
							return;
						}
					}

					// コンボボックスにない場合
					ICookieGetter newGetter = CookieGetter.CreateInstance(value);
					if (newGetter != null) {
						cmbBrowser.Items.Add(newGetter);
						cmbBrowser.SelectedItem = newGetter;
					}
				}
			}
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			this.CookieStatus = Utility.Deserialize(SETTING_FILE) as CookieStatus;
			backgroundWorker1.RunWorkerAsync();
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			Utility.Serialize(SETTING_FILE, this.CookieStatus);
		}

		private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
		{
			for (_ayncUpdateIndex = 0; _ayncUpdateIndex < cmbBrowser.Items.Count; _ayncUpdateIndex++) {
				lock (_asyncObj) {
					if (e.Cancel) { break; }
					if (_ayncUpdateIndex < cmbBrowser.Items.Count) {
						ICookieGetter cookieGetter = cmbBrowser.Items[_ayncUpdateIndex] as ICookieGetter;
						if (cookieGetter != null && cookieGetter.Status.IsAvailable) {
							if (e.Cancel) { break; }
							string name = Utility.GetUserName(cookieGetter);
							if (e.Cancel) { break; }
							if (name != null) {
								cookieGetter.Status.DisplayName = string.Format("{0} ({1})", cookieGetter.Status.Name, name);
								backgroundWorker1.ReportProgress(0);
							}
						}
					}
				}

			}

		}

		private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			int selectedIndex = cmbBrowser.SelectedIndex;
			object[] x = new object[cmbBrowser.Items.Count];
			cmbBrowser.Items.CopyTo(x, 0);
			cmbBrowser.Items.Clear();
			cmbBrowser.Items.AddRange(x);
			cmbBrowser.SelectedIndex = selectedIndex;
		}

		private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			_ayncUpdateIndex = 0;
		}

		private void cbShowAll_CheckedChanged(object sender, EventArgs e)
		{
			lock (_asyncObj) {
				_ayncUpdateIndex = 0;

				backgroundWorker1.CancelAsync();
				cmbBrowser.Items.Clear();
				cmbBrowser.Items.AddRange(CookieGetter.CreateInstances(!cbShowAll.Checked));
				if (cmbBrowser.Items.Count != 0) {
					cmbBrowser.SelectedIndex = 0;
					if (!backgroundWorker1.IsBusy) {
						backgroundWorker1.RunWorkerAsync();
					}
				} 

			}
		}

		private void cmbBrowser_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (cmbBrowser.SelectedItem is ICookieGetter) {
				ICookieGetter cookieGetter = (ICookieGetter)cmbBrowser.SelectedItem;
				txtPath.Text = cookieGetter.Status.CookiePath;
				txtPath.Enabled = true;
				btnRef.Enabled = true;
				getUserSession(cookieGetter);
			} else {
				txtPath.Text = "";
				txtPath.Enabled = false;
				btnRef.Enabled = false;
			}
		}

		private void btnRef_Click(object sender, EventArgs e)
		{
			if (cmbBrowser.SelectedItem is ICookieGetter) {
				ICookieGetter cookieGetter = (ICookieGetter)cmbBrowser.SelectedItem;
				string path = txtPath.Text;
				if (cookieGetter.Status.PathType == PathType.Directory) {
					if (System.IO.Directory.Exists(path)) {
						folderBrowserDialog1.SelectedPath = cookieGetter.Status.CookiePath;
					}
					if (folderBrowserDialog1.ShowDialog() == DialogResult.OK) {
						txtPath.Text = folderBrowserDialog1.SelectedPath;
						cookieGetter.Status.CookiePath = folderBrowserDialog1.SelectedPath;
					}
				} else {
					if (System.IO.File.Exists(path)) {
						openFileDialog1.InitialDirectory = System.IO.Path.GetDirectoryName(path);
						openFileDialog1.FileName = path;
					}
					if (openFileDialog1.ShowDialog() == DialogResult.OK) {
						txtPath.Text = openFileDialog1.FileName;
						cookieGetter.Status.CookiePath = openFileDialog1.FileName;
					}
				}
			}
		}

		private void btnCheck_Click(object sender, EventArgs e)
		{
			lock (_asyncObj) {
				_ayncUpdateIndex = 0;
				if (!backgroundWorker1.IsBusy) {
					backgroundWorker1.RunWorkerAsync();
				}
			}
		}

		private void getUserSession(ICookieGetter cookieGetter) {

			System.Net.Cookie cookie = cookieGetter.GetCookie(new Uri("http://live.nicovideo.jp/"), "user_session");

			if(cookie != null) {
				txtUserSession.Text = cookie.Value;
			}
			else {
				txtUserSession.Clear();
			}

		}

	}


}