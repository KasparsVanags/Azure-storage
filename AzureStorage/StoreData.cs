using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using static AzureStorage.Constants;

namespace AzureStorage
{
    public class StoreData
    {
        private readonly BlobContainerClient _blobContainerClient;
        private readonly TableClient _tableClient;

        public StoreData(BlobServiceClient blobServiceClient, TableServiceClient tableServiceClient)
        {
            _tableClient = tableServiceClient.GetTableClient(TABLE_NAME);
            _blobContainerClient = blobServiceClient.GetBlobContainerClient(CONTAINER_NAME);
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
                var content = webclient.OpenRead(DATA_SOURCE);
                nextIndex = GetNextIndex(_tableClient);
                var blob = _blobContainerClient.GetBlobClient(nextIndex);
                await blob.UploadAsync(content);
                success = true;
            }
            catch (Exception ex)
            {
                log.LogInformation(ex.ToString());
            }

            var tableEntry = new Record
            {
                RowKey = nextIndex,
                PartitionKey = PARTITION_KEY,
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