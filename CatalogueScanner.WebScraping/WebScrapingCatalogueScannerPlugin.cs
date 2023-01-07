using CatalogueScanner.Core.Host;
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
                .AddSingleton<PlaywrightBrowserManager>();

            AddConfiguration(builder);
        }

        private static void AddConfiguration(ICatalogueScannerHostBuilder builder)
        {
            var webScrapingSection = builder.Configuration.GetSection("WebScraping");
        }
    }
}
