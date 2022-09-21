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
        public static readonly IConfigurationRoot Config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("Properties/serviceDependencies.json").Build();
        
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddAzureClients(client =>
            {
                client.AddBlobServiceClient(Config.GetSection("Storage"));
                client.AddTableServiceClient(Config.GetSection("Table"));
                client.UseCredential(new DefaultAzureCredential());
            });
        }
    }
}