using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace SunokoLibrary.Application
{
    /// <summary>
    /// Cookie取得用の項目の保持
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{Name,nq}({ProfileName,nq}): {CookiePath}")]
    public class BrowserConfig
    {
        /// <param name="profileName">ブラウザのプロファイル名</param>
        /// <param name="cookiePath">ブラウザのCookieファイルパス</param>
        public BrowserConfig(string name, string profileName, string cookiePath, bool isCustomized = false)
        {
            BrowserName = name;
            ProfileName = profileName;
            CookiePath = cookiePath;
            IsCustomized = isCustomized;
        }

        /// <summary>
        /// ユーザーによるカスタム設定かを取得する。
        /// </summary>
        public bool IsCustomized { get; private set; }
        /// <summary>
        /// ブラウザ名を取得する。
        /// </summary>
        public string BrowserName { get; private set; }
        /// <summary>
        /// 識別名を取得する。
        /// </summary>
        public string ProfileName { get; private set; }
        /// <summary>
        /// クッキーが保存されているフォルダを取得、設定する。
        /// </summary>
        public string CookiePath { get; private set; }
        /// <summary>
        /// 引数で指定された値で上書きしたコピーを生成する。
        /// </summary>
        public BrowserConfig GenerateCopy(string name = null, string profileName = null, string cookiePath = null)
        { return new BrowserConfig(name ?? BrowserName, profileName ?? ProfileName, cookiePath ?? CookiePath, true); }
        public override int GetHashCode()
        { return string.Format("{0}{1}{2}", BrowserName, ProfileName, CookiePath).GetHashCode(); }
        public override bool Equals(object obj)
        {
            var target = obj as BrowserConfig;
            return (object)target != null
                ? (target.BrowserName == BrowserName && target.ProfileName == ProfileName && target.CookiePath == CookiePath) : false;
        }
        public static bool operator ==(BrowserConfig valueA, BrowserConfig valueB)
        {
            if(object.ReferenceEquals(valueA, valueB))
                return true;
            if((object)valueA == null || (object)valueB == null)
                return false;
            return valueA.Equals(valueB);
        }
        public static bool operator !=(BrowserConfig valueA, BrowserConfig valueB)
        { return !(valueA == valueB); }
    }
}
