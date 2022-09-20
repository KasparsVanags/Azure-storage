using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureStorage
{
    public class Api
    {
        private readonly BlobContainerClient _blobContainerClient;

        private readonly TableClient _tableClient;

        public Api(BlobServiceClient blobServiceClient, TableServiceClient tableServiceClient)
        {
            _blobContainerClient = blobServiceClient.GetBlobContainerClient("blob");
            _tableClient = tableServiceClient.GetTableClient("log");
        }

        [FunctionName("getLogs")]
        public async Task<IActionResult> GetLogs(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]
            HttpRequest req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string from = req.Query["from"];
            string to = req.Query["to"];

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            from ??= data?.from;
            to ??= data?.to;

            if (from == null || to == null)
                return new BadRequestObjectResult(
                    "Please pass a date from and to on the query string or in the request body");

            var timeFrom = DateTimeOffset.Parse(from);
            var timeTo = DateTimeOffset.Parse(to);

            return new OkObjectResult(_tableClient.Query<Record>()
                .Where(x => x.Timestamp != null &&
                            ((DateTimeOffset)x.Timestamp).DateTime >= timeFrom &&
                            ((DateTimeOffset)x.Timestamp).DateTime <= timeTo));
        }

        [FunctionName("getBlob")]
        public async Task<IActionResult> GetBlob(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]
            HttpRequest req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name ??= data?.name;

            return name != null
                ? new OkObjectResult(await _blobContainerClient.GetBlobClient(name).OpenReadAsync())
                : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        }
    }
}