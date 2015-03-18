using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SunokoLibrary.Windows.Forms
{
    using SunokoLibrary.Application;
    using SunokoLibrary.Windows.ViewModels;

    /// <summary>
    /// ニコニコ動画アカウント一覧の表示用コンボボックス。
    /// </summary>
    public class NicoSessionComboBox : BrowserComboBox
    {
#pragma warning disable 1591
        protected override void InitLayout()
        {
            base.InitLayout();
            Initialize(new CookieSourceSelector(CookieGetters.Default, importer => new NicoAccountSelectorItem(importer)));
        }
#pragma warning restore 1591

        class NicoAccountSelectorItem : CookieSourceItem
        {
            public NicoAccountSelectorItem(ICookieImporter importer) : base(importer) { }
            string _accountName, _displayText;
            public string AccountName
            {
                get { return _accountName; }
                private set
                {
                    _accountName = value;
                    OnPropertyChanged();
                }
            }
            public override string DisplayText
            {
                get { return _displayText; }
                protected set
                {
                    _displayText = value;
                    OnPropertyChanged();
                }
            }
            public async override void Initialize()
            {
                var baseText = string.Format("{0}{1}{2}",
                    Importer.SourceInfo.IsCustomized ? "カスタム設定 " : string.Empty,
                    Importer.SourceInfo.BrowserName,
                    Importer.SourceInfo.ProfileName.ToLowerInvariant() == "default" ? string.Empty : string.Format(" {0}", Importer.SourceInfo.ProfileName));
                DisplayText = string.Format("{0} (loading...)", baseText);
                AccountName = await GetUserName(Importer);
                DisplayText = string.IsNullOrEmpty(AccountName) == false
                    ? string.Format("{0} ({1})", baseText, AccountName) : baseText;
            }
            static async Task<string> GetUserName(ICookieImporter cookieImporter)
            {
                try
                {
                    var url = new Uri("http://www.nicovideo.jp/my/channel");
                    var container = new CookieContainer();
                    var client = new HttpClient(new HttpClientHandler() { CookieContainer = container });
                    var result = await cookieImporter.GetCookiesAsync(url);
                    if (result.AddTo(container) != CookieImportState.Success)
                        return null;

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
        }
    }
}