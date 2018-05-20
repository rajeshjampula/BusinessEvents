using System;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Json;

namespace ServiceBusSender
{   
    public static class ServiceBusReceiveProcessSend
    {
        const string ServiceBusConnectionString = "Endpoint=sb://demonamespacesb.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=33C5MVgYEz3Ox38+6SJZ0BomXxY9r2GPgJWlCw7DXTk=";
        const string QueueName = "inbound";
        const string TopicName = "outboundfiltering";
        static IMessageSender messageSender;
        static IMessageReceiver messageReceiver;

        [FunctionName("ServiceBusReceiveProcessSend")]
        public static void Run([TimerTrigger("*/10 * * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");            

            ReceiveAndProcess(log).GetAwaiter().GetResult();
        }

        static async Task ReceiveAndProcess(TraceWriter log)
        {
            const int numberOfMessages = 10;
            messageSender = new MessageSender(ServiceBusConnectionString, TopicName);
            messageReceiver = new MessageReceiver(ServiceBusConnectionString, QueueName, ReceiveMode.PeekLock,null,numberOfMessages);

            // Register QueueClient's MessageHandler and receive messages in a loop
            await ReceiveMessagesAsync(numberOfMessages, log);

            // Send Messages
            //await SendMessagesAsync(numberOfMessages);            

            await messageSender.CloseAsync();
            await messageReceiver.CloseAsync();
        }


        static async Task ReceiveMessagesAsync(int numberOfMessagesToReceive, TraceWriter tw)
        {
            //while (numberOfMessagesToReceive-- > 0)
            //{
            // Receive the message
            IList<Message> receiveList = await messageReceiver.ReceiveAsync(numberOfMessagesToReceive);
            foreach(Message msg in receiveList)
            {
                MemoryStream stream = new MemoryStream(msg.Body);
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(TheMsgBody));                
                var body = (TheMsgBody) ser.ReadObject(stream);

                Message NewMessage = new Message();
                NewMessage.Body = msg.Body;
                NewMessage.MessageId = msg.MessageId;

                // Process the message
                if (body.SalesValue > 9999)
                {                    
                    tw.Info("AWESOME - Big sales incoming!");                                        
                }                    
                else
                    tw.Info($"Regular sales incoming...");

                NewMessage.UserProperties.Add("SalesValue", body.SalesValue);
                await messageSender.SendAsync(NewMessage);
                await messageReceiver.CompleteAsync(msg.SystemProperties.LockToken);                               
            }                        

            // Complete the message so that it is not received again.
            // This can be done only if the MessageReceiver is created in ReceiveMode.PeekLock mode (which is default).            
            //}
        }
    }
}
