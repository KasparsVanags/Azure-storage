using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using static AzureStorage.Startup;

namespace AzureStorage
{
    public class StoreData
    {
        private readonly BlobContainerClient _blobContainerClient;
        private readonly TableClient _tableClient;

        public StoreData(BlobServiceClient blobServiceClient, TableServiceClient tableServiceClient)
        {
            _tableClient = tableServiceClient.GetTableClient(Config["Table:TableName"]);
            _blobContainerClient = blobServiceClient.GetBlobContainerClient(Config["Storage:BlobContainerName"]);
        }

        [FunctionName("StoreData")]
        public async Task Run([TimerTrigger("* * * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            var success = false;
            var nextIndex = "0";
            try
            {
                await _tableClient.CreateIfNotExistsAsync();
                await _blobContainerClient.CreateIfNotExistsAsync();
                var webclient = new WebClient();
                var content = webclient.OpenRead(Config["Source:Address"]);
                nextIndex = GetNextIndex(_tableClient);
                var blob = _blobContainerClient.GetBlobClient(nextIndex);
                await blob.UploadAsync(content);
                success = true;
            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
            }

            var tableEntry = new Record
            {
                RowKey = nextIndex,
                PartitionKey = Config["Table:PartitionKey"],
                Success = success
            };

            await _tableClient.AddEntityAsync(tableEntry);
        }

        private string GetNextIndex(TableClient tableClient)
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
    }
}