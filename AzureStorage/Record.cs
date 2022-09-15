using System;
using Azure;
using Azure.Data.Tables;

namespace AzureStorage
{
    public record Record : ITableEntity
    {
        public bool Success { get; set; }
        public string RowKey { get; set; } = default!;

        public string PartitionKey { get; set; } = default!;

        public ETag ETag { get; set; } = default!;

        public DateTimeOffset? Timestamp { get; set; } = default!;
    }
}