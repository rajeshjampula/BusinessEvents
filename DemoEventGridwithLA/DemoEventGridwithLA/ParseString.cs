using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace DemoEventGridwithLA
{
    public static class ParseString
    {
        [FunctionName("ParseString")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            // parse requests query parameters
            //string EventGridSubject = req.GetQueryNameValuePairs()
            // .FirstOrDefault(q => string.Compare(q.Key, "EventGridSubject", true) == 0)
            //.Value;

            // Get request body
            //dynamic data = await req.Content.ReadAsAsync<object>();
            //dynamic data2 = @"/blobServices/default/containers/demoblob/blobs/metrics-eh-1.png"; //After local debugging for debugging online
            string bodyContent = await req.Content.ReadAsStringAsync();

            // Set name to query string or body data
            /*name = name ?? data?.name;

            return name == null
                ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a name on the query string or in the request body")
                : req.CreateResponse(HttpStatusCode.OK, "Hello " + name);
            */
            // Query string
            //EventGridSubject = EventGridSubject ?? data?.EventGridSubject;

            string[] BlobAddressArray = bodyContent.Split(new string[] { @"/" }, System.StringSplitOptions.RemoveEmptyEntries);
            string BlobAddress = @"https://demoeg.blob.core.windows.net/" + BlobAddressArray[3] + "/" + BlobAddressArray[5];

            return req.CreateResponse(BlobAddress);
        }
    }
}
