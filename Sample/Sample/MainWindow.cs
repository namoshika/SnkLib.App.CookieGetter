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
            var tsk = nicoSessionComboBox1.Selector.SetConfigAsync(Properties.Settings.Default.SelectedBrowserConfig);
        }
        static readonly Uri TargetUrl = new Uri("http://live.nicovideo.jp/");

        async void Selector_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch(e.PropertyName)
            {
                case "SelectedIndex":
                    var cookieContainer = new CookieContainer();
                    var currentGetter = nicoSessionComboBox1.Selector.SelectedImporter;
                    var result = await currentGetter.GetCookiesAsync(TargetUrl, cookieContainer);
                    var cookie = cookieContainer.GetCookies(TargetUrl)["user_session"];

                    //UI更新
                    txtCookiePath.Text = currentGetter.Config.CookiePath;
                    txtCookiePath.Enabled = true;
                    btnOpenCookieFileDialog.Enabled = true;
                    txtUserSession.Text = cookie != null ? cookie.Value : null;
                    txtUserSession.Enabled = result == ImportResult.Success;

                    if (nicoSessionComboBox1.SelectedIndex >= 0
                        && nicoSessionComboBox1.SelectedIndex < nicoSessionComboBox1.Items.Count)
                    {
                        Properties.Settings.Default.SelectedBrowserConfig = currentGetter.Config;
                        Properties.Settings.Default.Save();
                    }
                    break;
            }
        }
        void btnReload_Click(object sender, EventArgs e)
        { var tsk = nicoSessionComboBox1.Selector.UpdateAsync(); }
        void btnOpenCookieFileDialog_Click(object sender, EventArgs e)
        { var tsk = nicoSessionComboBox1.ShowCookieDialogAsync(); }
        void checkBoxShowAll_CheckedChanged(object sender, EventArgs e)
        { nicoSessionComboBox1.IsAllBrowserMode = checkBoxShowAll.Checked; }
    }
}