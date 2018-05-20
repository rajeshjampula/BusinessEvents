using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using System.Diagnostics;


namespace SBthroughputTest
{
    class Program
    {
        // SB Connection
        string ServiceBusConnectionString = "Endpoint=sb://d365prototyping.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=lOpVLIzYfE2zT0O9cQqpTR5DtK6b7XwSW6Hwx5tDAfs=";
        string InboundToAX = "inbound";
        string OutboundFromAX = "outbound";
        IQueueClient queueClientInbound;
        IQueueClient queueClientOutbound;

        static void Main(string[] args)
        {            
            MainAsync().GetAwaiter().GetResult();
        }

        public static async Task MainAsync()
        {
            Program p = new Program();
            p.queueClientOutbound = new QueueClient(p.ServiceBusConnectionString, p.OutboundFromAX);
            p.queueClientInbound = new QueueClient(p.ServiceBusConnectionString, p.InboundToAX);

            // Specify Nr of Messages
            await p.SendMessages(50);
            await p.ShovelMessages();

            ConsoleKeyInfo cki;
            // Prevent example from ending if CTL+C is pressed.
            Console.TreatControlCAsInput = true;

            Console.WriteLine("Press any key to receive in this app.\nPress the Escape (Esc) key to quit\n");
            cki = Console.ReadKey();
                
            if (cki.Key != ConsoleKey.Escape)
            {
                await p.ReceiveMessages();
            }
        }

        public async Task SendMessages(int NrOfMsgs)
        {

            Stopwatch st = new Stopwatch();
            st.Start();

            List<Message> MessageList = new List<Message>();
            for(int i = 0;i<NrOfMsgs;i++)
            {             
                Guid g = Guid.NewGuid();
                string account = "SB" + g.ToString();

                var mBody = new Record
                {
                    theCompany = "USMF",
                    theCurrency = "USD",
                    theCustGroup = "10",
                    CustomerAccount = account
                };
                
                Message SbMessage = mBody.AsMessage();
                SbMessage.MessageId = account;

                MessageList.Add(SbMessage);
            }
            st.Stop();
            TimeSpan ts = st.Elapsed;

            // Format and display the TimeSpan value.
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Console.WriteLine("Prepare List elapsed time: " + elapsedTime);


            Stopwatch st2 = new Stopwatch();
            st2.Start();
            // Send the message to the queue
            await queueClientOutbound.SendAsync(MessageList);
            st2.Stop();
            TimeSpan ts2 = st2.Elapsed;

            // Format and display the TimeSpan value.
            string elapsedTime2 = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts2.Hours, ts2.Minutes, ts2.Seconds,
                ts2.Milliseconds / 10);
            Console.WriteLine("Sending List elapsed time: " + elapsedTime2);

            /*
             * Stopwatch st3 = new Stopwatch();
            st3.Start();
            // Send the message to the queue
            await queueClientOutbound.SendAsync(MessageList);
            st3.Stop();
            TimeSpan ts3 = st3.Elapsed;

            // Format and display the TimeSpan value.
            string elapsedTime3 = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts3.Hours, ts3.Minutes, ts3.Seconds,
                ts3.Milliseconds / 10);
            Console.WriteLine("Sending List elapsed time: " + elapsedTime3);
            */
        }

        public async Task ShovelMessages()
        {
            Stopwatch st = new Stopwatch();
            st.Start();
            var receiver = new MessageReceiver(ServiceBusConnectionString, OutboundFromAX, ReceiveMode.PeekLock, RetryPolicy.Default, 100);
            List<Message> MessagesToShovel = new List<Message>();

            while (true)
            {
                try
                {
                    IList<Message> messages = await receiver.ReceiveAsync(10, TimeSpan.FromSeconds(2));
                    

                    if (messages.Any())
                    {
                        foreach (var message in messages)
                        {                                                        
                            Message SbMessage = new Message();
                            SbMessage.Body = message.Body;
                            SbMessage.MessageId = message.MessageId;

                            MessagesToShovel.Add(SbMessage);
                            await receiver.CompleteAsync(message.SystemProperties.LockToken);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            st.Stop();
            TimeSpan ts = st.Elapsed;

            // Format and display the TimeSpan value.
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Console.WriteLine("Receive and prepare new List elapsed time: " + elapsedTime);

            await receiver.CloseAsync();
            Stopwatch st2 = new Stopwatch();
            st2.Start();

            await queueClientInbound.SendAsync(MessagesToShovel);

            st2.Stop();
            TimeSpan ts2 = st2.Elapsed;

            // Format and display the TimeSpan value.
            string elapsedTime2 = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts2.Hours, ts2.Minutes, ts2.Seconds,
                ts2.Milliseconds / 10);
            Console.WriteLine("Sending 2nd List elapsed time: " + elapsedTime2);
        }

        public async Task ReceiveMessages()
        {
            Stopwatch st = new Stopwatch();
            st.Start();
            var receiver = new MessageReceiver(ServiceBusConnectionString, InboundToAX, ReceiveMode.PeekLock, RetryPolicy.Default, 100);            

            while (true)
            {
                try
                {
                    IList<Message> messages = await receiver.ReceiveAsync(10, TimeSpan.FromSeconds(2));

                    if (messages.Any())
                    {
                        foreach (var message in messages)
                        {
                            Message MyMessage = message.As<Message>();

                            await receiver.CompleteAsync(message.SystemProperties.LockToken);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }

            st.Stop();
            TimeSpan ts = st.Elapsed;

            // Format and display the TimeSpan value.
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Console.WriteLine("Receive messages inbound elapsed time: " + elapsedTime);

            await receiver.CloseAsync();

            Console.ReadLine();
        }
    }
}
