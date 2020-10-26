using Azure.Identity;
using CatalogueScanner.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Hosting;
using System;

namespace CatalogueScanner.ConfigurationUI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            const string connectionStringPropertyName = "AzureAppConfigurationConnectionString";
            const string refresherSupplierPropertyName = "ConfigurationRefresher";

            var hostBuilder = Host.CreateDefaultBuilder(args);

            return hostBuilder
                  .ConfigureServices(services =>
                  {
                      services.SetAzureAppConfigurationConnectionString((string)hostBuilder.Properties[connectionStringPropertyName]);
                      services.SetConfigurationRefresher((Func<IConfigurationRefresher>)hostBuilder.Properties[refresherSupplierPropertyName]);
                  })
                  .ConfigureAppConfiguration((hostingContext, config) =>
                  {
                      var appConfig = config.Build();

                      var connectionString = appConfig["ConnectionStrings:AzureAppConfiguration"];

                      hostBuilder.Properties[connectionStringPropertyName] = connectionString;

                      config.AddCatalogueScannerAzureAppConfiguration(connectionString, out var refresherSupplier);

                      hostBuilder.Properties[refresherSupplierPropertyName] = refresherSupplier;

                      
                      var vaultUri = Environment.GetEnvironmentVariable("VaultUri");

                      if (vaultUri is null)
                      {
                          throw new InvalidOperationException("VaultUri environment variable not set");
                      }

                      config.AddAzureKeyVault(new Uri(vaultUri), new DefaultAzureCredential());
                  })
                  .ConfigureWebHostDefaults(webBuilder =>
                  {
                      webBuilder.UseStartup<Startup>();
                  });
        }
    }
}
