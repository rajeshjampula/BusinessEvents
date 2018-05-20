using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.ServiceBus;
using System.Runtime.Serialization.Json;

namespace ServiceBusSender
{    
    public class TheMsgBody
    {        
        public string Customer;
        public Int32 SalesValue;
    }

    public static class ServiceBusSender
    {
        const string ServiceBusConnectionString = "Endpoint=sb://demonamespacesb.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=33C5MVgYEz3Ox38+6SJZ0BomXxY9r2GPgJWlCw7DXTk=";
        const string QueueName = "inbound";
        static IQueueClient queueClient;

        [FunctionName("ServiceBusSender")]
        public static void Run([TimerTrigger("*/10 * * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");

            SendMessages(log).GetAwaiter().GetResult();
        }

        static async Task SendMessages(TraceWriter tw)
        {
            const int numberOfMessages = 10;
            queueClient = new QueueClient(ServiceBusConnectionString, QueueName);            

            // Send Messages
            await SendMessagesAsync(numberOfMessages, tw);

            await queueClient.CloseAsync();
        }

        static async Task SendMessagesAsync(int numberOfMessagesToSend, TraceWriter tw)
        {
            try
            {
                string messageId = "";
                int x = 0;
                for (var i = 0; i < numberOfMessagesToSend; i++)
                {
                    // Create a new message to send to the queue
                    var message = new Message();

                    TheMsgBody mB = new TheMsgBody
                    {
                        Customer = "Customer " + i.ToString()
                    };

                    if ((i % 2) == 0)
                    {
                        messageId = Guid.NewGuid().ToString();                        

                        if(x==1)
                        {
                            mB.SalesValue = 1000;                            
                            x = 0;
                        }
                        else
                            mB.SalesValue = 10000;

                        x++;                                                
                    }               
                    
                    message.MessageId = messageId;
                    MemoryStream stream1 = new MemoryStream();
                    DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(TheMsgBody));
                    ser.WriteObject(stream1, mB);                    
                    message.Body = stream1.ToArray(); 

                    // Write the body of the message to the console
                    tw.Info($"Sending message: ID:{messageId}");

                    // Send the message to the queue
                    await queueClient.SendAsync(message);
                }
            }
            catch (Exception exception)
            {
                tw.Info($"{DateTime.Now} :: Exception: {exception.Message}");
            }
        }
    }
}

