using CatalogueScanner.ColesOnline;
using CatalogueScanner.Configuration;
using CatalogueScanner.Core.Host;
using CatalogueScanner.Core;
using CatalogueScanner.Core.Host.ApplicationInsights;
using CatalogueScanner.Localisation.OrchardCore;
using CatalogueScanner.SaleFinder;
using CatalogueScanner.WoolworthsOnline;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.DurableTask.Worker;

#region Replace Console.Error
#pragma warning disable CA2000 // Dispose objects before losing scope
// Replace the Console.Error stream to record error output from Playwright in Application Insights
var telemetryClient = new TelemetryClient(new TelemetryConfiguration
{
    ConnectionString = Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING")
});

var errorStream = new ApplicationInsightsStream(10240, telemetryClient);

Console.SetError(TextWriter.Synchronized(new StreamWriter(errorStream)
{
    AutoFlush = true,
}));
#pragma warning restore CA2000 // Dispose objects before losing scope
#endregion

#region Install Playwright Browsers
var playwrightBrowsersPath = Environment.GetEnvironmentVariable("PLAYWRIGHT_BROWSERS_PATH")!;

// Create the Playwright browsers directory manually so we get a clear exception message if we don't have permission
Directory.CreateDirectory(playwrightBrowsersPath);

// Install the browser required by Playwright 
#pragma warning disable CA1861 // Avoid constant arrays as arguments
Microsoft.Playwright.Program.Main(new[] { "install", "chromium" });
#pragma warning restore CA1861 // Avoid constant arrays as arguments
#endregion

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration((context, builder) =>
    {
        var connectionString = Environment.GetEnvironmentVariable(CoreAppSettingNames.AzureAppConfigurationConnectionString);

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException($"{CoreAppSettingNames.AzureAppConfigurationConnectionString} app setting is required");
        }

        builder.AddCatalogueScannerAzureAppConfiguration(connectionString);
    })
    .ConfigureServices((context, services) =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddAzureAppConfiguration();

        services.Configure<DurableTaskWorkerOptions>(options => options.EnableEntitySupport = true);

        ICatalogueScannerHostBuilder catalogueScannerHostBuilder = new CatalogueScannerHostBuilder(context.Configuration, services);

        catalogueScannerHostBuilder
            .AddPlugin<CoreCatalogueScannerPlugin>()
            .AddPlugin<OrchardCoreLocalisationCatalogueScannerPlugin>()
            .AddPlugin<ColesOnlineCatalogueScannerPlugin>()
            .AddPlugin<SaleFinderCatalogueScannerPlugin>()
            .AddPlugin<WoolworthsOnlineCatalogueScannerPlugin>();
    })
    .Build();

host.Run();
