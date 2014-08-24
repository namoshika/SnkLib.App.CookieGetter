using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace SunokoLibrary.Application.Browsers
{
    /// <summary>
    /// 指定したブラウザからクッキーを取得する
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{Config.Name,nq}({Config.ProfileName,nq}): {Config.CookiePath}")]
    public abstract class CookieGetterBase : ICookieImporter
    {
        public CookieGetterBase(BrowserConfig option, PathType cookiePathType)
        {
            if (option == null)
                throw new ArgumentNullException("引数statusがnullです。");
            Config = option;
            CookiePathType = cookiePathType;
        }

        public BrowserConfig Config { get; private set; }
        public PathType CookiePathType{get;private set;}
        public virtual bool IsAvailable
        {
            get
            {
                return string.IsNullOrEmpty(Config.CookiePath)
                    ? false : System.IO.File.Exists(Config.CookiePath);
            }
        }
        public abstract bool GetCookies(Uri targetUrl, CookieContainer container);
        public abstract ICookieImporter Generate(BrowserConfig config);
    }
}