using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;

namespace SunokoLibrary.Application
{
    class Program
    {
        static void Main(string[] args)
        {
            //直接起動されたような兆候がある場合は接客モード
            if (args.Length != 2)
            {
                Console.WriteLine("SnkLib.App.CookieGetter.x86Proxy: SnkLib.App.CookieGetterがx64向けアプリ下で動く際にx86環境下で呼び出す必要のあるAPIの代理実行をするプログラムです。");
                Console.ReadLine();
                return;
            }
            //親プロセスへのサービス公開完了通知を匿名パイプで行う
            using (var pipeClient = new System.IO.Pipes.AnonymousPipeClientStream(System.IO.Pipes.PipeDirection.Out, args[1]))
            {
                //名前付きパイプでサービスを公開する
                var endpointUrl = new Uri(string.Format("net.pipe://localhost/SnkLib.App.CookieGetter.x86Proxy/{0}/Service/", args[0]));
                Console.WriteLine("EndpointUrl: {0}", endpointUrl);
                var host = new ServiceHost(typeof(Service), endpointUrl);
                host.AddServiceEndpoint(typeof(IProxyService), new NetNamedPipeBinding(), endpointUrl);
                try
                {
                    host.Open();
                    pipeClient.WriteByte(byte.MaxValue);
                    //クライアントには指定時間以内に要件を済ませる事を要求する
                    System.Threading.Thread.Sleep(TimeSpan.FromSeconds(3));
                }
                catch (AddressAlreadyInUseException) { pipeClient.WriteByte(byte.MaxValue); }
                catch (Exception)
                {
                    pipeClient.WriteByte(byte.MaxValue);
                    throw;
                }
                finally { host.Close(); }
            }
        }
    }
}