using System;
using Avro.File;
using Avro.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Text;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Blob; // Namespace for Blob storage types

// NOTE: Capture keeps running even if there is no new data to capture based on the time interval so if there is no high ingestion volume but a fast capture frequency empty blobs may get created.
namespace ConsumeAvroFilesFromCapture
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();            
        }

        private static async Task MainAsync(string[] args)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(@"DefaultEndpointsProtocol=https;AccountName=demosaehandsb;AccountKey=M/E07gdsv/wRTGokKnmPAmrS+M9F3hPBYqR+46kCsHwG4YlVwYQSf8b1DIwIsRSTCM0oiazbNbDFeBDg9latQw==;EndpointSuffix=core.windows.net");

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference("democapture");            

            Console.WriteLine("Iterating through blobs:");
            BlobContinuationToken continuationToken = null;
            BlobResultSegment resultSegment = null;            

            do
            {
                //This overload allows control of the page size. You can return all remaining results by passing null for the maxResults parameter,
                //or by calling a different overload.
                resultSegment = await container.ListBlobsSegmentedAsync("", true, BlobListingDetails.All, 5000, continuationToken, null, null);                
                foreach (var blobItem in resultSegment.Results)
                {
                    //Console.WriteLine("\t{0}", blobItem.StorageUri.PrimaryUri);
                    string[] theURI = blobItem.Uri.Segments;
                    string blobReference = "";
                    for(var i = 0; i < theURI.Length; i++)
                    {
                        if(i>=2)
                        {
                            blobReference += theURI[i];
                        }
                    }
                    
                    CloudBlockBlob myBlockBlob = container.GetBlockBlobReference(blobReference);

                    MemoryStream memoryStream = new MemoryStream();
                    await myBlockBlob.DownloadRangeToStreamAsync(memoryStream, null, null);                        
                   
                    if(myBlockBlob.Properties.Length>0)
                    {
                        //string pathSource = @"C:\temp\37.avro";
                        using (var reader = Avro.File.DataFileReader<GenericRecord>.OpenReader(memoryStream))
                        {
                            while (reader.HasNext())
                            {
                                GenericRecord record = reader.Next();
                               
                                long sn = (long)record["SequenceNumber"];                                
                                string offset = (string)record["Offset"];
                                

                                byte[] body = (byte[])record["Body"];
                                string bodyText = Encoding.UTF8.GetString(body);
                                Console.WriteLine($"{sn}: {offset}, {bodyText}");
                                // process other property according to the schema: https://docs.microsoft.com/en-us/azure/event-hubs/event-hubs-capture-overview
                            }
                        }
                    }                   
                }                

                //Get the continuation token.
                continuationToken = resultSegment.ContinuationToken;
            }
            while (continuationToken != null);

            Console.WriteLine("Press any key to exit");
            Console.ReadLine();            
        }        
    }
}
