using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;

namespace Hal.CookieGetterSharp {

    class Program {
        static void Main(string[] args) {
            var id = args.Length > 0 ? args[0] : "null";
            var endpointUrl = new Uri(string.Format("net.pipe://localhost/CookieGetterSharp.x86Proxy/{0}/Service/", id));
            System.Console.WriteLine(endpointUrl);

            //名前付きパイプでサービスを公開する
            var host = new ServiceHost(typeof(Service), endpointUrl);
            host.AddServiceEndpoint(typeof(IX86ProxyService), new NetNamedPipeBinding(), endpointUrl);
            try {
                host.Open();
                //サービスは指定時間以内に要件を済ませる事を要求する
                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(5));
            }
            catch(AddressAlreadyInUseException) { }
            finally { host.Close(); }
        }
    }
}
