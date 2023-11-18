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
            ArgumentNullException.ThrowIfNull(builder);
            #endregion

            builder.Services
                .AddSingleton<PlaywrightBrowserManager>();

            AddConfiguration(builder);
        }

        private static void AddConfiguration(ICatalogueScannerHostBuilder builder)
        {
            _ = builder.Configuration.GetSection("WebScraping");
        }
    }
}
