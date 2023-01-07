﻿using CatalogueScanner.ColesOnline;
using CatalogueScanner.Configuration;
using CatalogueScanner.Core;
using CatalogueScanner.Core.Host;
using CatalogueScanner.Core.Host.ApplicationInsights;
using CatalogueScanner.DefaultHost;
using CatalogueScanner.Localisation.OrchardCore;
using CatalogueScanner.SaleFinder;
using CatalogueScanner.WoolworthsOnline;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
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

#pragma warning disable CA2000 // Dispose objects before losing scope
            // Replace the Console.Error stream to record error output from Playwright in Application Insights
            var telemetryClient = new TelemetryClient(new TelemetryConfiguration(Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY")));
            var errorStream = new ApplicationInsightsStream(10240, telemetryClient);

            Console.SetError(TextWriter.Synchronized(new StreamWriter(errorStream)
            {
                AutoFlush = true,
            }));
#pragma warning restore CA2000 // Dispose objects before losing scope

            var playwrightBrowsersPath = Environment.GetEnvironmentVariable("PLAYWRIGHT_BROWSERS_PATH")!;

            // Create the Playwright browsers directory manually so we get a clear exception message if we don't have permission
            Directory.CreateDirectory(playwrightBrowsersPath);

            // Install the browser required by Playwright 
            Microsoft.Playwright.Program.Main(new[] { "install", "chromium" });

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
                .AddPlugin<ColesOnlineCatalogueScannerPlugin>()
                .AddPlugin<SaleFinderCatalogueScannerPlugin>()
                .AddPlugin<WoolworthsOnlineCatalogueScannerPlugin>();
        }
    }
}