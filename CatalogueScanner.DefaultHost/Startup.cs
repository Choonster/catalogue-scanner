using CatalogueScanner.Configuration;
using CatalogueScanner.Core;
using CatalogueScanner.Core.Host;
using CatalogueScanner.DefaultHost;
using CatalogueScanner.Localisation.OrchardCore;
using CatalogueScanner.SaleFinder;
using CatalogueScanner.WebScraping;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

[assembly: FunctionsStartup(typeof(Startup))]

namespace CatalogueScanner.DefaultHost
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            #region null checks
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            #endregion

            var errorStream = new TracingStream(10240, LogLevel.Error);

#pragma warning disable CA2000 // Dispose objects before losing scope
            Console.SetError(new StreamWriter(errorStream)
            {
               AutoFlush = true,
            });
#pragma warning restore CA2000 // Dispose objects before losing scope

            Microsoft.Playwright.Program.Main(new[] { "install chromium" });

            var connectionString = Environment.GetEnvironmentVariable("AzureAppConfigurationConnectionString");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("AzureAppConfigurationConnectionString app setting is required");
            }

            var configuration = new ConfigurationBuilder()
                .AddCatalogueScannerAzureAppConfiguration(connectionString, out var refresherSupplier)
                .Build();

            var localConfiguration = new ConfigurationBuilder()
                .AddEnvironmentVariables("CatalogueScanner:")
                .Build();

            ICatalogueScannerHostBuilder catalogueScannerHostBuilder = new CatalogueScannerHostBuilder(builder, configuration, localConfiguration);

            catalogueScannerHostBuilder.Services.SetConfigurationRefresher(refresherSupplier);

            catalogueScannerHostBuilder
                .AddPlugin<CoreCatalogueScannerPlugin>()
                .AddPlugin<OrchardCoreLocalisationCatalogueScannerPlugin>()
                .AddPlugin<SaleFinderCatalogueScannerPlugin>()
                .AddPlugin<WebScrapingCatalogueScannerPlugin>();
        }
    }
}