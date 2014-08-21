using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.ServiceModel;
using System.Text;

namespace Hal.CookieGetterSharp {
    public class Service : IX86ProxyService {
        public int GetCookiesFromProtectedModeIE(out string cookiesText, Uri targetUrl, string valueKey = null) {
            return win32api.GetCookiesFromProtectedModeIE(out cookiesText, targetUrl, valueKey);
        }
    }
}
