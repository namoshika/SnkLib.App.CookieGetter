using System.Configuration;

namespace Sample.Properties
{
    internal sealed partial class Settings
    {
        [UserScopedSetting, SettingsSerializeAs(SettingsSerializeAs.String)]
        public SunokoLibrary.Application.BrowserConfig SelectedBrowserConfig
        {
            get { return (SunokoLibrary.Application.BrowserConfig)this["SelectedBrowserConfig"]; }
            set { this["SelectedBrowserConfig"] = value; }
        }
    }
}