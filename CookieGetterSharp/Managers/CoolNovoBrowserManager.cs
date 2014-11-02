using SunokoLibrary.Application.Browsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Win32;


namespace Hal.CookieGetterSharp
{
    class CoolNovoBrowserManager : BlinkBrowserManager
    {
        public CoolNovoBrowserManager()
            : base("CoolNovo", null)
        {
            string folder = null;
            try
            {
                //Vista 7以外は別の場所になるので、インストールフォルダーを捜す
                // レジストリ・キーのパスを指定してレジストリを開く
                RegistryKey rKey = Registry.CurrentUser.OpenSubKey(rKeyName);
                if (rKey != null)
                {
                    // レジストリの値を取得
                    string location = (string)rKey.GetValue(rGetValueName);

                    // 開いたレジストリ・キーを閉じる
                    rKey.Close();

                    // コンソールに取得したレジストリの値を表示
                    if (location != null)
                    {
                        folder = Path.Combine(location, ProfileFolder);

                        if (!System.IO.Directory.Exists(folder))
                        {
                            folder = null;
                        }
                    }
                }
                else
                {
                    folder = null;
                }
            }
            catch (TypeInitializationException)
            {
                folder = null;
            }
            catch (NullReferenceException)
            {
                folder = null;
            }

            if (folder == null)
            {
                folder = Path.Combine(Utility.ReplacePathSymbols(BaseFolder), ProfileFolder);
                if (!System.IO.Directory.Exists(folder))
                {
                    folder = null;
                }
            }

            DataFolder = folder;
        }
        const string rKeyName = @"Software\ChromePlus";
        const string rGetValueName = "Install_Dir";

        // プロファイル対応 4/28
        private readonly string BaseFolder = "%LOCALAPPDATA%\\MapleStudio\\";
        private readonly string ProfileFolder = @"ChromePlus\User Data";
    }
}