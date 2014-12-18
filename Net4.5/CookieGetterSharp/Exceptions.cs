using System;
using System.Collections.Generic;
using System.Text;

namespace Hal.CookieGetterSharp
{
    /// <summary>
    /// クッキー取得に関する例外
    /// </summary>
    [global::System.Serializable]
    public class CookieGetterException : Exception
    {
        public CookieGetterException(SunokoLibrary.Application.CookieImportException inner)
            : base(inner.Message, inner) { }
        
        protected CookieGetterException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}