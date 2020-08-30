using CatalogueScanner.Configuration;
using CatalogueScanner.Core;
using CatalogueScanner.Core.Host;
using CatalogueScanner.DefaultHost;
using CatalogueScanner.Localisation.OrchardCore;
using CatalogueScanner.SaleFinder;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System;

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

            var connectionString = Environment.GetEnvironmentVariable("AzureAppConfigurationConnectionString");

            if (string.IsNullOrEmpty(connectionString))
            {
                // Localisation isn't available at this stage
#pragma warning disable CA1303 // Do not pass literals as localized parameters
                throw new InvalidOperationException("AzureAppConfigurationConnectionString app setting is required");
#pragma warning restore CA1303 // Do not pass literals as localized parameters
            }

            var configuration = new ConfigurationBuilder()
                .AddCatalogueScannerAzureAppConfiguration(connectionString)
                .Build();

            ICatalogueScannerHostBuilder catalogueScannerHostBuilder = new CatalogueScannerHostBuilder(builder, configuration);

            catalogueScannerHostBuilder
                .AddPlugin<CoreCatalogueScannerPlugin>()
                .AddPlugin<OrchardCoreLocalisationCatalogueScannerPlugin>()
                .AddPlugin<SaleFinderCatalogueScannerPlugin>();
        }
    }
}