using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using System.IO;
using Newtonsoft.Json;
using System.Text;

namespace SendToAXviaSB
{
    class Program
    {
        // SB Connection
        string ServiceBusConnectionString = "Endpoint=sb://d365prototyping.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=lOpVLIzYfE2zT0O9cQqpTR5DtK6b7XwSW6Hwx5tDAfs=";
        string InboundToAX = "outbound";

        static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }

        public static async Task MainAsync()
        {
            Program p = new Program();
            var sender = new MessageSender(p.ServiceBusConnectionString, p.InboundToAX);            

            await p.SendMessages(sender, 10, false);           

            Console.WriteLine("Press key to continue");
            Console.ReadLine();
        }

        private async Task SendMessages(MessageSender sender, int NrOfMsgs, bool measureTime)
        {
            Stopwatch st = new Stopwatch();

            for (int i = 0; i < NrOfMsgs; i++)
            {
                if (measureTime)
                {
                    st.Start();
                }

                Guid g = Guid.NewGuid();
                string account = "SB" + g.ToString();

                var mBody = new Record
                {
                    theCompany = "USMF",
                    theCurrency = "USD",
                    theCustGroup = "10",
                    CustomerAccount = account
                };

                Message SbMessage = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(mBody)));
                SbMessage.MessageId = account;                

                await sender.SendAsync(SbMessage);

                if (measureTime)
                {
                    st.Stop();
                    TimeSpan ts = st.Elapsed;

                    // Format and display the TimeSpan value.
                    string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                        ts.Hours, ts.Minutes, ts.Seconds,
                        ts.Milliseconds / 10);
                    Console.WriteLine($"Sending message {i} elapsed time: " + elapsedTime);
                    st.Reset();
                }
            }
        }
    }
}
