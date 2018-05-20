using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;

namespace DemoSBToAX
{
    public static class Sender
    {
        const string ServiceBusConnectionString = "Endpoint=sb://demonamespacesb.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=33C5MVgYEz3Ox38+6SJZ0BomXxY9r2GPgJWlCw7DXTk=";
        const string QueueName = "sendtod365fo";
        static IQueueClient queueClient;

        [FunctionName("Sender")]
        public static void Run([TimerTrigger("*/30 * * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");

            SendMessages(log).GetAwaiter().GetResult();
        }

        static async Task SendMessages(TraceWriter tw)
        {
            
            queueClient = new QueueClient(ServiceBusConnectionString, QueueName);

            // Send Messages
            await SendMessagesAsync(tw);

            await queueClient.CloseAsync();
        }

        static async Task SendMessagesAsync(TraceWriter tw)
        {
            try
            {
                string messageId = "";                                
                    // Create a new message to send to the queue
                var message = new Message();

                var mBody = new MessageBody();

                Guid g = Guid.NewGuid();

                mBody.Company = "USMF";
                mBody.Currency = "USD";
                mBody.CustGroup = "10";
                mBody.CustomerAccount = g.ToString();

                message.MessageId = g.ToString();                
                string jsonBody = JsonConvert.SerializeObject(mBody);
                message.Body = Encoding.UTF8.GetBytes(jsonBody);
                
                // Write the body of the message to the console
                tw.Info($"Sending message: ID:{g.ToString()}");

                // Send the message to the queue
                await queueClient.SendAsync(message);                
            }
            catch (Exception exception)
            {
                tw.Info($"{DateTime.Now} :: Exception: {exception.Message}");
            }
        }
    }
}
