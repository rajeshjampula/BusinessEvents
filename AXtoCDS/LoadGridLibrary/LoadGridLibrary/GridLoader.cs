using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.EventGrid;
using Microsoft.Azure.EventGrid.Models;
using System.Diagnostics;
using Newtonsoft.Json;

namespace LoadGridLibrary
{
    class myRecord
    {
        public string CustomerAccount { get; set; }
        public string theCurrency { get; set; }
        public string theCustGroup { get; set; }
        public string theCompany { get; set; }
    }

    public class GridLoader
    {
        public static int SubmitEventsToGrid(int howMany)
        {            
            var credentials = new TopicCredentials("ZjvlWXjewB4UAu+BikWkV7cw/TJYirWn0SH+mW6UOis=");

            var client = new EventGridClient(credentials);

            var events = new List<EventGridEvent>();

            Stopwatch st = new Stopwatch();
            st.Start();
            //Prepare data
            for (int i = 1; i <= howMany; i++)
            {
                string theSubject = "";
                string theGuid = "";

                // LA and Functions
                /*if (i % 2 == 0)
                {
                    theGuid = "Log" + Guid.NewGuid().ToString();
                    theSubject = "LogicApps";
                }
                else
                {
                    theGuid = "Fun" + Guid.NewGuid().ToString();
                    theSubject = "Functions";
                }  */

                // Relay
                theGuid = "Log" + Guid.NewGuid().ToString();
                theSubject = "Relay";


                var mr = new myRecord
                {
                    theCompany = "USMF",
                    theCurrency = "USD",
                    theCustGroup = "10",
                    CustomerAccount = theGuid
                };

                var recordData = JsonConvert.SerializeObject(mr);

                var eventGridEvent = new EventGridEvent
                {
                    Subject = theSubject,
                    EventType = "my-event",
                    EventTime = DateTime.UtcNow,
                    Id = Guid.NewGuid().ToString(),
                    Data = recordData,
                    DataVersion = "1.0.0"
                };

                events.Add(eventGridEvent);
            }
            st.Stop();
            TimeSpan ts = st.Elapsed;

            // Format and display the TimeSpan value.
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);            

            Stopwatch st2 = new Stopwatch();
            st2.Start();
            // Send
            client.PublishEventsWithHttpMessagesAsync("anothertopic.westus2-1.eventgrid.azure.net", events);

            st2.Stop();
            TimeSpan ts2 = st2.Elapsed;

            // Format and display the TimeSpan value.
            string elapsedTime2 = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts2.Hours, ts2.Minutes, ts2.Seconds,
                ts2.Milliseconds / 10);

            return howMany;
        }
    }
}
