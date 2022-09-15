using System;
using Azure;
using Azure.Data.Tables;
using Azure.Identity;
using Azure.Storage.Blobs;

namespace AzureStorage
{
    public static class Client
    {
        public static readonly BlobContainerClient BlobContainer = new(
            new Uri("https://codelex.blob.core.windows.net/blob"),
            new DefaultAzureCredential());

        public static readonly TableClient Table = new(new Uri("https://codelex.table.core.windows.net/log"),
            new AzureSasCredential("?sv=2021-06-08&ss=bfqt&srt=sco&sp=rwdlacupiytfx&se=2023-09-14T03:47:21Z&" +
                                   "st=2022-09-13T19:47:21Z&spr=https," +
                                   "http&sig=%2BzBcnatiA55dFWKRgTfWopiuACzYyXjYPVYOdUzh9IU%3D"));
    }
}