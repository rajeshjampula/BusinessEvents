using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.EventHubs;

namespace EventHubsSender
{
    public static class SenderUsingCron
    {
        private static EventHubClient eventHubClient;
        private const string EventHubConnectionString = "Endpoint=sb://demonamespaceeh.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=+QG31wAUl4E0NF3CdGHyaq+h1qF7uyuGe1I8eBD9lVk=";
        private const string EventHubName = "demoehcapture";        

        [FunctionName("SenderUsingCron")]
        public static void Run([TimerTrigger("*/5 * * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");

            SendMessagesAsync(log).GetAwaiter().GetResult();
        }

        private static async Task SendMessagesAsync(TraceWriter tw)
        {
            // Creates an EventHubsConnectionStringBuilder object from a the connection string, and sets the EntityPath.
            // Typically the connection string should have the Entity Path in it, but for the sake of this simple scenario
            // we are using the connection string from the namespace.
            var connectionStringBuilder = new EventHubsConnectionStringBuilder(EventHubConnectionString)
            {
                EntityPath = EventHubName
            };

            eventHubClient = EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());

            await SendMessagesToEventHub(10, tw);

            await eventHubClient.CloseAsync();

            tw.Info($"Send loop complete: {DateTime.Now}");
        }

        // Creates an Event Hub client and sends 100 messages to the event hub.
        private static async Task SendMessagesToEventHub(int numMessagesToSend,TraceWriter tw)
        {
            for (var i = 0; i < numMessagesToSend; i++)
            {
                try
                {
                    var message = $"Message {i}";
                    tw.Info($"Sending message: {message}");
                    await eventHubClient.SendAsync(new EventData(Encoding.UTF8.GetBytes(message)));
                }
                catch (Exception exception)
                {
                    tw.Info($"{DateTime.Now} > Exception: {exception.Message}");
                }

                await Task.Delay(10);
            }

            tw.Info($"{numMessagesToSend} messages sent.");
        }
    }
}
