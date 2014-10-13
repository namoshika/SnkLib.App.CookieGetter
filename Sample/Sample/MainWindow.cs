using SunokoLibrary.Application;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sample
{
    public partial class MainWindow : Form
    {
        public MainWindow()
        {
            InitializeComponent();
            var a = Observable.Merge(
                Observable.Return(Unit.Default),
                Observable.FromEventPattern(
                    handler => checkBoxShowAll.CheckedChanged += handler,
                    handler => checkBoxShowAll.CheckedChanged -= handler).Select(eArgs => Unit.Default),
                Observable.FromEventPattern(
                    handler => btnReload.Click += handler,
                    handler => btnReload.Click -= handler).Select(eArgs => Unit.Default))
                .Subscribe(idx => RefreshBrowseSelector());
            var b = Observable.FromEventPattern(
                handler => cmbBxBrowserSelector.SelectedIndexChanged += handler,
                handler => cmbBxBrowserSelector.SelectedIndexChanged -= handler).Select(eArgs => Unit.Default)
                .Subscribe(unit => RefreshBrowseSelector(cmbBxBrowserSelector.SelectedIndex));
            Observable.FromEventPattern<FormClosedEventHandler, FormClosedEventArgs>(
                handler => new FormClosedEventHandler(handler),
                handler => FormClosed += handler,
                handler => FormClosed -= handler).Select(eArgs => Unit.Default)
                .Subscribe(unit =>
                {
                    a.Dispose();
                    b.Dispose();
                });
        }
        bool _inited = true;
        System.Threading.SemaphoreSlim _semaph = new System.Threading.SemaphoreSlim(1);
        BrowserSelectItem[] _browsers;
        bool _addedCustom;

        async void RefreshBrowseSelector()
        {
            try
            {
                await _semaph.WaitAsync();
                var selectedBrowserItem = cmbBxBrowserSelector.SelectedIndex >= 0
                    ? _browsers[cmbBxBrowserSelector.SelectedIndex] : null;
                await Task.Run(() => _browsers = CookieGetters.CreateInstances(!checkBoxShowAll.Checked)
                    .Select(getter => new BrowserSelectItem() { Getter = getter }).ToArray());

                //ニックネーム取得
                var nicknames = Task.Factory
                    .ContinueWhenAll(_browsers.Select(item => item.Getter.IsAvailable ? GetUserName(item.Getter) : Task.FromResult<string>(null)).ToArray(),
                    tsks => tsks.Select((tsk, idx) => new { Value = tsk.Result, Index = idx }));
                foreach (var nickname in await nicknames)
                    _browsers[nickname.Index].AccountName = nickname.Value;

                //UI更新
                cmbBxBrowserSelector.Items.Clear();
                cmbBxBrowserSelector.Items.AddRange(_browsers.Select(item => item.DisplayName).ToArray());
                var configs = _browsers.Select(item => item.Getter.Config).ToList();
                //選択項目の設定
                if (_inited)
                {
                    //前回起動時の設定を復元
                    _inited = false;
                    var currentGetter = CookieGetters.CreateInstance(new BrowserConfig(
                        Sample.Properties.Settings.Default.BrowserName,
                        Sample.Properties.Settings.Default.ProfileName,
                        Sample.Properties.Settings.Default.CookiePath));
                    if (currentGetter.Config.IsCustomized == false)
                        cmbBxBrowserSelector.SelectedIndex = configs.IndexOf(currentGetter.Config);
                    else
                    {
                        _browsers = _browsers.Concat(new[] { new BrowserSelectItem() }).ToArray();
                        cmbBxBrowserSelector.Items.Add(string.Empty);
                        _browsers[_browsers.Length - 1].Getter = currentGetter;
                        _browsers[_browsers.Length - 1].AccountName = await GetUserName(currentGetter);
                        cmbBxBrowserSelector.Items[cmbBxBrowserSelector.Items.Count - 1] = _browsers[_browsers.Length - 1].DisplayName;
                        cmbBxBrowserSelector.SelectedIndex = cmbBxBrowserSelector.Items.Count - 1;
                    }
                }
                else
                {
                    //更新前に選択していた項目を再選択させる
                    var selectedIndex = selectedBrowserItem != null ? configs.IndexOf(selectedBrowserItem.Getter.Config) : -1;
                    cmbBxBrowserSelector.SelectedIndex =
                        Math.Max(cmbBxBrowserSelector.Items.Count > 0 ? 0 : -1,
                        Math.Min(cmbBxBrowserSelector.Items.Count - 1, selectedIndex));
                }
            }
            finally
            { _semaph.Release(); }
        }
        async void RefreshBrowseSelector(int index)
        {
            try
            {
                await _semaph.WaitAsync();
                var browserItem = _browsers[index];
                var cookieGetter = browserItem.Getter;
                var cookieContainer = new CookieContainer();
                var targetUrl = new Uri("http://live.nicovideo.jp/");
                cookieGetter.GetCookies(targetUrl, cookieContainer);
                var cookie = cookieContainer.GetCookies(targetUrl)["user_session"];

                //UI更新
                txtCookiePath.Text = cookieGetter.Config.CookiePath;
                txtCookiePath.Enabled = true;
                btnOpenCookieFileDialog.Enabled = true;
                txtUserSession.Text = cookie != null ? cookie.Value : null;

                if(cmbBxBrowserSelector.SelectedIndex >= 0 && cmbBxBrowserSelector.SelectedIndex < _browsers.Length)
                {
                    Properties.Settings.Default.BrowserName = _browsers[cmbBxBrowserSelector.SelectedIndex].Getter.Config.BrowserName;
                    Properties.Settings.Default.ProfileName = _browsers[cmbBxBrowserSelector.SelectedIndex].Getter.Config.ProfileName;
                    Properties.Settings.Default.CookiePath = _browsers[cmbBxBrowserSelector.SelectedIndex].Getter.Config.CookiePath;
                    Properties.Settings.Default.Save();
                }
            }
            finally
            { _semaph.Release(); }
        }
        static async Task<string> GetUserName(ICookieImporter cookieGetter)
        {
            try
            {
                var url = new Uri("http://www.nicovideo.jp/my/channel");
                var container = new CookieContainer();
                var client = new HttpClient(new HttpClientHandler() { CookieContainer = container });
                cookieGetter.GetCookies(url, container);
                var res = await client.GetStringAsync(url);

                if (string.IsNullOrEmpty(res))
                    return null;
                var namem = Regex.Match(res, "nickname = \"([^<>]+)\";", RegexOptions.Singleline);
                if (namem.Success)
                    return namem.Groups[1].Value;
                else
                    return null;
            }
            catch (System.Net.Http.HttpRequestException) { return null; }
        }

        async void btnOpenCookieFileDialog_Click(object sender, EventArgs e)
        {
            var selectedBrowser = _browsers[cmbBxBrowserSelector.SelectedIndex];
            var path = txtCookiePath.Text;
            ICookieImporter getter = null;
            DialogResult res;
            switch (selectedBrowser.Getter.CookiePathType)
            {
                case PathType.Directory:
                    if (System.IO.Directory.Exists(path))
                        folderBrowserDialog1.SelectedPath = selectedBrowser.Getter.Config.CookiePath;
                    if ((res = folderBrowserDialog1.ShowDialog()) == DialogResult.OK)
                    {
                        txtCookiePath.Text = folderBrowserDialog1.SelectedPath;
                        getter = selectedBrowser.Getter.Generate(
                            selectedBrowser.Getter.Config.GenerateCopy(cookiePath: folderBrowserDialog1.SelectedPath));
                    }
                    break;
                case PathType.File:
                    if (System.IO.File.Exists(path))
                    {
                        openFileDialog1.InitialDirectory = System.IO.Path.GetDirectoryName(path);
                        openFileDialog1.FileName = path;
                    }
                    if ((res = openFileDialog1.ShowDialog()) == DialogResult.OK)
                    {
                        txtCookiePath.Text = openFileDialog1.FileName;
                        getter = selectedBrowser.Getter.Generate(
                            selectedBrowser.Getter.Config.GenerateCopy(cookiePath: openFileDialog1.FileName));
                    }
                    break;
                default:
                    return;
            }

            if (res == System.Windows.Forms.DialogResult.OK)
            {
                if (_addedCustom == false)
                {
                    _browsers = _browsers.Concat(new[] { new BrowserSelectItem() }).ToArray();
                    cmbBxBrowserSelector.Items.Add(string.Empty);
                    _addedCustom = true;
                }
                _browsers[_browsers.Length - 1].Getter = getter;
                _browsers[_browsers.Length - 1].AccountName = await GetUserName(getter);
                cmbBxBrowserSelector.Items[cmbBxBrowserSelector.Items.Count - 1] = _browsers[_browsers.Length - 1].DisplayName;
                cmbBxBrowserSelector.SelectedIndex = cmbBxBrowserSelector.Items.Count - 1;
            }
        }
    }
    class BrowserSelectItem
    {
        public ICookieImporter Getter { get; set; }
        public string AccountName { get; set; }
        public string DisplayName
        {
            get
            {
                return (Getter.Config.IsCustomized ? "カスタム設定 " : string.Empty) + Getter.Config.BrowserName
                    + (string.IsNullOrEmpty(AccountName) ? string.Empty : string.Format("({0})", AccountName));
            }
        }
    }
}