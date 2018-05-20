using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace FunctionSendingToAX
{
    public static class CreateCustomer
    {
        [FunctionName("CreateCustomer")]
        public static void Run([TimerTrigger("*/1 * * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");

            InsertRow(log).GetAwaiter().GetResult();
        }

        static async Task InsertRow(TraceWriter tw)
        {
            string ServicePath = "/api/services/DataAccess/InsertCustTable/InsertCT";

            string OperationPath = string.Format("{0}{1}", Config.Default.UriString.TrimEnd('/'), ServicePath);

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(Config.Default.UriString);
            client.DefaultRequestHeaders.Add(Auth.OAuthHeader, await Auth.GetAuthenticationHeader(true));

            Guid g = Guid.NewGuid();
            string account = "New" + g.ToString();

            var mBody = new Record
            {                
                theCompany = "USMF",
                theCurrency = "USD",
                theCustGroup = "10",
                CustomerAccount =  account              
            };

            // Create a request
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, ServicePath);
            request.Content = new StringContent(JsonConvert.SerializeObject(mBody), Encoding.UTF8, "application/json");

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
}
