using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace Hal.CookieGetterSharp {
    [ServiceContract]
    internal interface IX86ProxyService {
        [OperationContract]
        int GetCookiesFromProtectedModeIE(out string cookiesText, Uri targetUrl, string valueKey = null);
    }
}