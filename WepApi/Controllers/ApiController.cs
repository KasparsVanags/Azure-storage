using static AzureStorage.Client;
using AzureStorage;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WepApi.Controllers
{
    [Route("api")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        [Route("getLogs")]
        [HttpGet]
        public List<Record> GetLogs(string from = "1.1.1", string to = "9999.9.9")
        {
            var timeFrom = DateTimeOffset.Parse(from);
            var timeTo = DateTimeOffset.Parse(to);
            return Table.Query<Record>()
                .Where(x => ((DateTimeOffset)x.Timestamp).DateTime >= timeFrom &&
                            ((DateTimeOffset)x.Timestamp).DateTime <= timeTo).ToList();
        }

        [Route("getBlob")]
        [HttpGet]
        public Stream GetBlob(string name)
        {
            if(name == null) return null;
            var blobClient = BlobContainer.GetBlobClient(name);
            return blobClient.OpenRead();
        }
    }
}
