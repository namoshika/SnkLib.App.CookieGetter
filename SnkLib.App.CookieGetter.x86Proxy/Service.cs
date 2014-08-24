using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.ServiceModel;
using System.Text;

namespace SunokoLibrary.Application
{
    class Service : IProxyService
    {
        public int GetCookiesFromProtectedModeIE(out string cookiesText, Uri targetUrl, string valueKey = null)
        { return Win32Api.GetCookiesFromProtectedModeIE(out cookiesText, targetUrl, valueKey); }
    }
}
