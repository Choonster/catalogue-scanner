using CatalogueScanner.Configuration;
using CatalogueScanner.Core.Host;
using CatalogueScanner.WebScraping.API.Options;
using CatalogueScanner.WebScraping.API.Service;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace CatalogueScanner.WebScraping.API
{
    public class WebScrapingApiCatalogueScannerPlugin : ICatalogueScannerPlugin
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
                .AddScoped<ColesOnlineService>();

            AddConfigurationOptions(builder);
        }

        private static void AddConfigurationOptions(ICatalogueScannerHostBuilder builder)
        {
            var webScrapingApiSection = builder.Configuration.GetSection("WebScrapingApi");

            builder.Services
                .ConfigureOptions<ColesOnlineOptions>(webScrapingApiSection.GetSection(ColesOnlineOptions.ColesOnline));
        }
    }
}
