using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace SunokoLibrary.Application
{
    /// <summary>
    /// 親プロセスと異なるモード下で動かしている子プロセスが公開する機能を定義します。
    /// </summary>
    [ServiceContract]
    interface IProxyService
    {
        [OperationContract]
        int GetCookiesFromProtectedModeIE(out string cookiesText, Uri targetUrl, string valueKey = null);
    }
}