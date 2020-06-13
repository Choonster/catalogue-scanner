using CatalogueScanner.Core.Host;
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

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var appConfig = config.Build();
                    config.AddCatalogueScannerAzureAppConfiguration(appConfig["ConnectionStrings:AzureAppConfiguration"]);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
