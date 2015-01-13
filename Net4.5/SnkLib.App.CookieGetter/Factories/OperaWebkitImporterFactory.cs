using System;
using System.Collections.Generic;
using System.Text;

namespace SunokoLibrary.Application.Browsers
{
    /// <summary>
    /// OperaからICookieImporterを取得します。
    /// </summary>
    public class OperaWebkitImporterFactory : BlinkImporterFactory
    {
#pragma warning disable 1591
        public OperaWebkitImporterFactory()
            : base("Opera Webkit", "%APPDATA%\\Opera Software\\Opera Stable", defaultFolder: string.Empty) { }
#pragma warning restore 1591
    }
}