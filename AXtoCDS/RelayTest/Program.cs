namespace RelayTest
{
    using System;
    using System.IO;    
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.Azure.Relay;
    using System.Linq;

    public class Program
    {
        private const string RelayNamespace = "d365prototp.servicebus.windows.net";
        private const string ConnectionName = "getegdata";
        private const string KeyName = "RootManageSharedAccessKey";
        private const string Key = "FSFffEq38RWrqMp3S2cb0UemjFJ6nNeNw1u5yu3CP5c=";

        public static void Main(string[] args)
        {
            RunAsync().GetAwaiter().GetResult();
        }

        private static async Task RunAsync()
        {
            var tokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(KeyName, Key);
            var listener = new HybridConnectionListener(new Uri(string.Format("sb://{0}/{1}", RelayNamespace, ConnectionName)), tokenProvider);

            // Subscribe to the status events.
            listener.Connecting += (o, e) => { Console.WriteLine("Connecting"); };
            listener.Offline += (o, e) => { Console.WriteLine("Offline"); };
            listener.Online += (o, e) => { Console.WriteLine("Online"); };

            // Provide an HTTP request handler
            listener.RequestHandler = (context) =>
            {
                // Do something with context.Request.Url, HttpMethod, Headers, InputStream...
                context.Response.StatusCode = HttpStatusCode.OK;
                context.Response.StatusDescription = "OK";
                /*using (var sw = new StreamWriter(context.Response.OutputStream))
                {
                    sw.WriteLine("hello!");
                }*/

                // The context MUST be closed here
                TraceRequest(context.Request);
                context.Response.Close();
            };

            // Opening the listener establishes the control channel to
            // the Azure Relay service. The control channel is continuously 
            // maintained, and is reestablished when connectivity is disrupted.
            await listener.OpenAsync();
            Console.WriteLine("Server listening");

            // Start a new thread that will continuously read the console.
            await Console.In.ReadLineAsync();

            // Close the listener after you exit the processing loop.
            await listener.CloseAsync();
        }

        static void TraceRequest(RelayedHttpListenerRequest request)
        {
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine($"{DateTime.UtcNow}: Received events for eghcintegrationtopic");

            Console.WriteLine($"{request.HttpMethod} {request.Url}");
            request.Headers.AllKeys.ToList().ForEach((k) => Console.WriteLine($"{k}: {request.Headers[k]}"));
            Console.WriteLine(new StreamReader(request.InputStream).ReadToEnd());
        }
    }
}
