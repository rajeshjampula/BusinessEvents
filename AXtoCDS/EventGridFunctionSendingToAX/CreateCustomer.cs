using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Collections.Generic;
using Newtonsoft.Json;
using Microsoft.Azure.EventGrid.Models;

namespace EventGridFunctionSendingToAX
{
    public static class CreateCustomer
    {
        [FunctionName("CreateCustomer")]        
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");
            // parse query parameter
            var content = req.Content;

            // Get content
            string jsonContent = await content.ReadAsStringAsync();
            log.Info($"Received Event with payload: {jsonContent}");

            IEnumerable<string> headerValues;
            if (req.Headers.TryGetValues("Aeg-Event-Type", out headerValues))
            {
                // Handle Subscription validation (Whenever you create a new subscription we send a new validation message)
                var validationHeaderValue = headerValues.FirstOrDefault();
                if (validationHeaderValue == "SubscriptionValidation")
                {
                    var events = JsonConvert.DeserializeObject<GridEvent[]>(jsonContent);
                    var code = events[0].Data["validationCode"];
                    return req.CreateResponse(HttpStatusCode.OK,
                    new { validationResponse = code });
                }
                // React to new messages and receive
                else
                {
                    //ReceiveAndProcess(log, JsonConvert.DeserializeObject<GridEvent[]>(jsonContent)).GetAwaiter().GetResult();
                    log.Info("Deserialize Data");                                        
                    EventGridEvent[] eventGridEvents = JsonConvert.DeserializeObject<EventGridEvent[]>(jsonContent);
                    log.Info("Send to AX");
                    await InsertRow(log, eventGridEvents[0].Data.ToString());
                }
            }

            return jsonContent == null
            ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a name on the query string or in the request body")
            : req.CreateResponse(HttpStatusCode.OK, jsonContent);
        }



        static async Task InsertRow(TraceWriter tw,string mBody)
        {
            string ServicePath = "/api/services/DataAccess/InsertCustTable/InsertCT";

            string OperationPath = string.Format("{0}{1}", Config.Default.UriString.TrimEnd('/'), ServicePath);

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(Config.Default.UriString);
            client.DefaultRequestHeaders.Add(Auth.OAuthHeader, await Auth.GetAuthenticationHeader(true));

            // Create a request            
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, ServicePath);
            request.Content = new StringContent(mBody, Encoding.UTF8, "application/json");            

            // Run the service
            var result = client.SendAsync(request).Result;

            // Display result to console
            if (result.IsSuccessStatusCode)
            {
                tw.Info(result.Content.ReadAsStringAsync().Result);
            }
            else
            {
                tw.Info(result.StatusCode.ToString());
            }

            Console.ReadLine();
        }
    }

    public class GridEvent
    {
        public string Id { get; set; }
        public string EventType { get; set; }
        public string Subject { get; set; }
        public System.DateTime EventTime { get; set; }
        public Dictionary<string, string> Data { get; set; }
        public string Topic { get; set; }
    }
}
