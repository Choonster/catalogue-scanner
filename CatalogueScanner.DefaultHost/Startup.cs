using CatalogueScanner.Configuration;
using CatalogueScanner.Core;
using CatalogueScanner.Core.Host;
using CatalogueScanner.DefaultHost;
using CatalogueScanner.Localisation.OrchardCore;
using CatalogueScanner.SaleFinder;
using CatalogueScanner.WebScraping;
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

            using var process = new System.Diagnostics.Process
            {
                StartInfo = new()
                {
                    FileName = "/bin/bash",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    Arguments = $"-c \"ls -l \\\"{typeof(Startup).Assembly.Location}\\\"",
                },
            };

            process.Start();
            process.WaitForExit();

            var stdout = process.StandardOutput.ReadToEnd();
            var stderr = process.StandardError.ReadToEnd();

            throw new Exception($"\n\nstdout: {stdout}\n\nstderr: {stderr}");
            

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