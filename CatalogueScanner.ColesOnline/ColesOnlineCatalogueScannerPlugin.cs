using CatalogueScanner.ColesOnline.Options;
using CatalogueScanner.ColesOnline.Service;
using CatalogueScanner.Configuration;
using CatalogueScanner.Core.Host;
using Microsoft.Extensions.DependencyInjection;

namespace CatalogueScanner.ColesOnline
{
    public class ColesOnlineCatalogueScannerPlugin : ICatalogueScannerPlugin
    {
        public void Configure(ICatalogueScannerHostBuilder builder)
        {
            #region null checks
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            #endregion

            builder.Services
                .AddHttpClient<ColesOnlineService>(client =>
                {
                    client.BaseAddress = new Uri("https://www.coles.com.au/_next/data/20221222.02_v3.19.0/en/");
                });

            AddConfiguration(builder);
        }

        private static void AddConfiguration(ICatalogueScannerHostBuilder builder)
        {
            var colesOnlineSection = builder.Configuration.GetSection("ColesOnline");

            var webScrapingSection = builder.Configuration.GetSection("WebScraping").GetSection(ColesOnlineOptions.ColesOnline);
            if (webScrapingSection.GetChildren().Any())
            {
                builder.Services.Configure<ColesOnlineOptions>(webScrapingSection);
            }

            builder.Services
                .ConfigureOptions<ColesOnlineOptions>(colesOnlineSection.GetSection(ColesOnlineOptions.ColesOnline));
        }
    }
}
