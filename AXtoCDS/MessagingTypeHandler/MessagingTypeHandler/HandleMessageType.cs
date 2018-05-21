using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;

namespace MessagingTypeHandler
{
    public static class HandleMessageType
    {
        public static string ReturnMessage(BrokeredMessage message)
        {
            var body = message.GetBody<Stream>();

            var retBody = new StreamReader(body, true).ReadToEnd();
            return retBody.ToString();
        }
    }

    // Need helper for getting the object to ax
    public static class DeserializeBody
    {
        public static CustTableRecord ReturnObject(String theJson)
        {            
            return JsonConvert.DeserializeObject<CustTableRecord>(theJson);
        }
    }

    // Need record representation for easy handling.
    public class CustTableRecord
    {
        public string CustomerAccount { get; set; }
        public string theCurrency { get; set; }
        public string theCustGroup { get; set; }
        public string theCompany { get; set; }
    }
}
