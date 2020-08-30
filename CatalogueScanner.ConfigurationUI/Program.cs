using CatalogueScanner.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

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

            var hostBuilder = Host.CreateDefaultBuilder(args);

            return hostBuilder
                  .ConfigureServices(services =>
                  {
                      services.SetAzureAppConfigurationConnectionString((string)hostBuilder.Properties[connectionStringPropertyName]);
                  })
                  .ConfigureAppConfiguration((hostingContext, config) =>
                  {
                      var appConfig = config.Build();

                      var connectionString = appConfig["ConnectionStrings:AzureAppConfiguration"];

                      hostBuilder.Properties[connectionStringPropertyName] = connectionString;

                      config.AddCatalogueScannerAzureAppConfiguration(connectionString);
                  })
                  .ConfigureWebHostDefaults(webBuilder =>
                  {
                      webBuilder.UseStartup<Startup>();
                  });
        }
    }
}
