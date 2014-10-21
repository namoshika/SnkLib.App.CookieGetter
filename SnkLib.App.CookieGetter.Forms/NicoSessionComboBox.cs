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

    /// <summary>
    /// ニコニコ動画アカウント一覧の表示用コンボボックス。
    /// </summary>
    public class NicoSessionComboBox : BrowserComboBox
    {
        public NicoSessionComboBox()
        { Selector = new BrowserSelector(getter => new NicoAccountSelectorItem(getter)); }
        class NicoAccountSelectorItem : BrowserItem
        {
            public NicoAccountSelectorItem(ICookieImporter getter) : base(getter) { }
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
            public async override Task InitializeAsync()
            {
                AccountName = await GetUserName(Getter);
                DisplayText = (Getter.Config.IsCustomized ? "カスタム設定 " : string.Empty) + Getter.Config.BrowserName
                    + (string.IsNullOrEmpty(AccountName) ? string.Empty : string.Format(" ({0})", AccountName));
            }
            static async Task<string> GetUserName(ICookieImporter cookieGetter)
            {
                try
                {
                    var url = new Uri("http://www.nicovideo.jp/my/channel");
                    var container = new CookieContainer();
                    var client = new HttpClient(new HttpClientHandler() { CookieContainer = container });
                    await cookieGetter.GetCookiesAsync(url, container);
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
