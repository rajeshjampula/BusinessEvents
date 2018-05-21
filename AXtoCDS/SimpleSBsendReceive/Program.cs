using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;

namespace SimpleSBsendReceive
{
    class Program
    {
        // SB Connection
        string ServiceBusConnectionString = "Endpoint=sb://d365prototyping.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=lOpVLIzYfE2zT0O9cQqpTR5DtK6b7XwSW6Hwx5tDAfs=";
        string InboundToAX = "lightweight";

        static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }

        public static async Task MainAsync()
        {
            Program p = new Program();           
            var sender = new MessageSender(p.ServiceBusConnectionString, p.InboundToAX);
            var receiver = new MessageReceiver(p.ServiceBusConnectionString, p.InboundToAX, ReceiveMode.PeekLock, RetryPolicy.Default, 10);

            // Specify Nr of Messages
            Stopwatch st = new Stopwatch();
            st.Start();

            await p.SendMessages(sender,1000, false);

            st.Stop();
            TimeSpan ts = st.Elapsed;

            // Format and display the TimeSpan value.
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Console.WriteLine($"Total send time: " + elapsedTime);

            st.Reset();
            st.Start();

            await p.ReceiveMessages(receiver, false);

            st.Stop();
            ts = st.Elapsed;

            // Format and display the TimeSpan value.
            elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Console.WriteLine($"Total receive time: " + elapsedTime);

            await p.SendMessagesMultiTask(sender,1000);
            await p.ReceiveMessagesMultiTask(receiver,100);

            // Send 25 batches of 4 messages each so 100 total
            await p.SendMessagesMultiTaskBatch(sender, 100, 10);

            // Receive batches with 25 tasks, each task attempts to receive 10
            await p.ReceiveMessagesMultiTaskBatch(receiver, 10, 100);

            await receiver.CloseAsync();

            Console.WriteLine("Press key to continue");
            Console.ReadLine();
        }

        public async Task SendMessagesMultiTaskBatch(MessageSender sender, int NrOfBatches,int MessagesPerBatch)
        {
            List<Task> SendMessageTaskList = new List<Task>();
            for (int i = 0; i < NrOfBatches; i++)
            {
                SendMessageTaskList.Add(SendMessagesBatch(sender, MessagesPerBatch));
            }

            Stopwatch st = new Stopwatch();
            st.Start();

            Task.WaitAll(SendMessageTaskList.ToArray());

            st.Stop();
            TimeSpan ts = st.Elapsed;

            // Format and display the TimeSpan value.
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Console.WriteLine($"{NrOfBatches} batches {MessagesPerBatch} messages each using tasks elapsed time: " + elapsedTime);
        }

        private async Task ReceiveMessagesMultiTaskBatch(MessageReceiver receiver, int ReceiveTasks, int batchSize)
        {
            List<Task> ReceiveMessageTaskList = new List<Task>();
            for (int i = 0; i < ReceiveTasks; i++)
            {
                ReceiveMessageTaskList.Add(ReceiveMessagesBatch(receiver, batchSize));
            }

            Stopwatch st = new Stopwatch();
            st.Start();

            Task.WaitAll(ReceiveMessageTaskList.ToArray());

            st.Stop();
            TimeSpan ts = st.Elapsed;

            // Format and display the TimeSpan value.
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Console.WriteLine($"{ReceiveTasks} receive tasks, attempt receive batches of {batchSize} elapsed time: " + elapsedTime);
        }

        public async Task SendMessagesMultiTask(MessageSender sender, int NrOfMsgs)
        {            
            List<Task> SendMessageTaskList = new List<Task>();
            for(int i = 0; i < NrOfMsgs; i++)
            {
                SendMessageTaskList.Add(SendMessages(sender));
            }

            Stopwatch st = new Stopwatch();
            st.Start();

            Task.WaitAll(SendMessageTaskList.ToArray());

            st.Stop();
            TimeSpan ts = st.Elapsed;

            // Format and display the TimeSpan value.
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Console.WriteLine($"{NrOfMsgs} messages send using tasks elapsed time: " + elapsedTime);            
        }
       
        private async Task ReceiveMessagesMultiTask(MessageReceiver receiver,int ReceiveTasks)
        {            
            List<Task> ReceiveMessageTaskList = new List<Task>();
            for (int i = 0; i < ReceiveTasks; i++)
            {
                ReceiveMessageTaskList.Add(ReceiveMessages(receiver,false));
            }

            Stopwatch st = new Stopwatch();
            st.Start();

            Task.WaitAll(ReceiveMessageTaskList.ToArray());

            st.Stop();
            TimeSpan ts = st.Elapsed;

            // Format and display the TimeSpan value.
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Console.WriteLine($"{ReceiveTasks} receive tasks elapsed time: " + elapsedTime);            
        }
        private async Task SendMessages(MessageSender sender)
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

            await sender.SendAsync(SbMessage);
        }

        private async Task SendMessages(MessageSender sender,int NrOfMsgs, bool measureTime)
        {
            Stopwatch st = new Stopwatch();

            for (int i = 0; i < NrOfMsgs; i++)
            {
                if(measureTime)
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

                Message SbMessage = mBody.AsMessage();
                SbMessage.MessageId = account;

                await sender.SendAsync(SbMessage);

                if(measureTime)
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

        private async Task ReceiveMessages(MessageReceiver receiver,bool reportTime = true)
        {            
            int i = 0;
            Stopwatch st = new Stopwatch();

            while (true)
            {
                try
                {
                    if(reportTime)
                    {                        
                        st.Start();
                    }
                   
                    Message message = await receiver.ReceiveAsync(TimeSpan.FromSeconds(3));

                    if (message != null)
                    {
                         Message MyMessage = message.As<Message>();

                         await receiver.CompleteAsync(message.SystemProperties.LockToken);
                         i++;
                    }
                    else
                    {
                        break;
                    }

                    if (reportTime)
                    {
                        st.Stop();
                        TimeSpan ts = st.Elapsed;

                        // Format and display the TimeSpan value.
                        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                            ts.Hours, ts.Minutes, ts.Seconds,
                            ts.Milliseconds / 10);
                        Console.WriteLine($"Received message {i} inbound elapsed time: " + elapsedTime);
                        st.Reset();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        public async Task SendMessagesBatch(MessageSender sender,int NrOfMessages)
        {
            List<Message> MessageList = new List<Message>();
            for (int i = 0; i < NrOfMessages; i++)
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
            
            // Send the message to the queue
            await sender.SendAsync(MessageList);            
        }


        public async Task ReceiveMessagesBatch(MessageReceiver receiver,int batchSize)
        {            
            while (true)
            {
                try
                {
                    IList<Message> messages = await receiver.ReceiveAsync(batchSize, TimeSpan.FromSeconds(2));

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
        }
    }
}
