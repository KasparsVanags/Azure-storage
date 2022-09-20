using System.IO;
using Azure.Identity;
using AzureStorage;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;

[assembly: FunctionsStartup(typeof(Startup))]

namespace AzureStorage
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("Properties/serviceDependencies.json").Build();
            builder.Services.AddAzureClients(client =>
            {
                client.AddBlobServiceClient(config.GetSection("Storage"));
                client.AddTableServiceClient(config.GetSection("Table"));
                client.UseCredential(new DefaultAzureCredential());
            });
        }
    }
}