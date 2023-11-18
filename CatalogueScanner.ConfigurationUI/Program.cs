using Azure.Identity;
using CatalogueScanner.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;

namespace CatalogueScanner.ConfigurationUI;

public static class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        const string connectionStringPropertyName = "AzureAppConfigurationConnectionString";

        var hostBuilder = Host.CreateDefaultBuilder(args);

        return hostBuilder
              .ConfigureServices(services =>
              {
                  services.SetAzureAppConfigurationConnectionString((string)hostBuilder.Properties[connectionStringPropertyName]);
              })
              .ConfigureAppConfiguration((hostingContext, config) =>
              {
                  var appConfig = config.Build();

                  var connectionString = appConfig["ConnectionStrings:AzureAppConfiguration"]
                    ?? throw new InvalidOperationException("ConnectionStrings:AzureAppConfiguration app setting not set");

                  hostBuilder.Properties[connectionStringPropertyName] = connectionString;

                  config.AddCatalogueScannerAzureAppConfiguration(connectionString);

                  if (hostingContext.HostingEnvironment.IsProduction())
                  {
                      var vaultUri = Environment.GetEnvironmentVariable("VaultUri")
                        ?? throw new InvalidOperationException("VaultUri environment variable not set");

                      config.AddAzureKeyVault(new Uri(vaultUri), new DefaultAzureCredential());
                  }
              })
              .ConfigureWebHostDefaults(webBuilder =>
              {
                  webBuilder.UseStartup<Startup>();
              });
    }
}
