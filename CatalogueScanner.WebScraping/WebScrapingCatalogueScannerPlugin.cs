using CatalogueScanner.Configuration;
using CatalogueScanner.Core.Host;
using CatalogueScanner.WebScraping.Options;
using CatalogueScanner.WebScraping.Service;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace CatalogueScanner.WebScraping
{
    public class WebScrapingCatalogueScannerPlugin : ICatalogueScannerPlugin
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

            AddConfiguration(builder);
        }

        private static void AddConfiguration(ICatalogueScannerHostBuilder builder)
        {
            var webScrapingSection = builder.Configuration.GetSection("WebScraping");

            builder.Services
                .ConfigureOptions<ColesOnlineOptions>(webScrapingSection.GetSection(ColesOnlineOptions.ColesOnline));
        }
    }
}
