using SunokoLibrary.Application;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
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
            nicoSessionComboBox1.Selector.PropertyChanged += Selector_PropertyChanged;
            var tsk = nicoSessionComboBox1.Selector.SetInfoAsync(Properties.Settings.Default.SelectedSourceInfo);
        }
        static readonly Uri TargetUrl = new Uri("http://live.nicovideo.jp/");

        async void Selector_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch(e.PropertyName)
            {
                case "SelectedIndex":
                    var cookieContainer = new CookieContainer();
                    var currentGetter = nicoSessionComboBox1.Selector.SelectedImporter;
                    if (currentGetter != null)
                    {
                        var result = await currentGetter.GetCookiesAsync(TargetUrl);
                        var cookie = result.Status == CookieImportState.Success ? result.Cookies["user_session"] : null;
                        //UI更新
                        txtCookiePath.Text = currentGetter.SourceInfo.CookiePath;
                        btnOpenCookieFileDialog.Enabled = true;
                        txtUserSession.Text = cookie != null ? cookie.Value : null;
                        txtUserSession.Enabled = result.Status == CookieImportState.Success;
                        Properties.Settings.Default.SelectedSourceInfo = currentGetter.SourceInfo;
                        Properties.Settings.Default.Save();
                    }
                    else
                    {
                        txtCookiePath.Text = null;
                        txtUserSession.Text = null;
                        txtUserSession.Enabled = false;
                        btnOpenCookieFileDialog.Enabled = false;
                    }
                    break;
            }
        }
        void btnReload_Click(object sender, EventArgs e)
        { var tsk = nicoSessionComboBox1.Selector.UpdateAsync(); }
        void btnOpenCookieFileDialog_Click(object sender, EventArgs e)
        { var tsk = nicoSessionComboBox1.ShowCookieDialogAsync(); }
        void checkBoxShowAll_CheckedChanged(object sender, EventArgs e)
        { nicoSessionComboBox1.Selector.IsAllBrowserMode = checkBoxShowAll.Checked; }
    }
}