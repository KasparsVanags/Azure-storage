using System;
using System.IO;
using System.Linq;
using System.Net;
using Azure.Data.Tables;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using static AzureStorage.Client;

namespace AzureStorage
{
    public static class StoreData
    {
        [FunctionName("StoreData")]
        public static void Run([TimerTrigger("* * * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            var success = false;
            var nextIndex = "";
            try
            {
                var content = FetchData("https://api.publicapis.org/random?auth=null");
                nextIndex = GetNextIndex(Table);
                var blob = BlobContainer.GetBlobClient(nextIndex);
                blob.Upload(GenerateStreamFromString(content));
                success = true;
            }
            catch (Exception ex)
            {
                log.LogInformation(ex.ToString());
            }

            var tableEntry = new Record
            {
                RowKey = nextIndex,
                PartitionKey = "idklol",
                Success = success
            };

            Table.AddEntity(tableEntry);
        }

        private static string FetchData(string address)
        {
            var webClient = new WebClient();
            return webClient.DownloadString(address);
        }

        private static string GetNextIndex(TableClient tableClient)
        {
            try
            {
                var lastIndex = tableClient.Query<Record>().Max(x => int.Parse(x.RowKey));
                return (lastIndex + 1).ToString();
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message != "Sequence contains no elements") throw;
                return "0";
            }
        }

        private static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}