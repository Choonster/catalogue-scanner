using CatalogueScanner.Dto.Config;
using CatalogueScanner.Localisation;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Localization;
using System;
using System.Globalization;

[assembly: FunctionsStartup(typeof(CatalogueScanner.Startup))]

namespace CatalogueScanner
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

            builder.Services.AddHttpClient();
            builder.Services.AddHttpClient<SaleFinderService>();

            #region Localisation
            builder.Services.AddMemoryCache();
            builder.Services.AddPortableObjectLocalization(o => o.ResourcesPath = "Localisation");
            builder.Services.AddSingleton<ILocalizationFileLocationProvider, FunctionsRootPoFileLocationProvider>();

            builder.Services.Configure<FunctionsPathOptions>(o =>
            {
                // https://github.com/Azure/azure-functions-dotnet-extensions/issues/17#issuecomment-499086297
                var executionContextOptions = builder.Services.BuildServiceProvider().GetService<IOptions<ExecutionContextOptions>>().Value;
                var appDirectory = executionContextOptions.AppDirectory;
                o.RootDirectory = appDirectory;
            });

            var localisationCulture = Environment.GetEnvironmentVariable("LocalisationCulture");
            if (localisationCulture != null)
            {
                var localisationCultureInfo = CultureInfo.GetCultureInfo(localisationCulture);
                CultureInfo.DefaultThreadCurrentCulture = localisationCultureInfo;
                CultureInfo.DefaultThreadCurrentUICulture = localisationCultureInfo;
            }
            #endregion

            #region Configuration
            IConfigurationRefresher? configurationRefresher = null;

            // Load configuration from Azure App Configuration
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddAzureAppConfiguration(options =>
            {
                options.Connect(Environment.GetEnvironmentVariable("AzureAppConfigurationConnectionString"))
                       // Load all keys that start with `CatalogueScanner:`
                       .Select("CatalogueScanner:*")
                       // Configure to reload configuration if the registered 'Sentinel' key is modified
                       .ConfigureRefresh(refreshOptions =>
                            refreshOptions.Register("CatalogueScanner:Settings:Sentinel", refreshAll: true)
                        );

                configurationRefresher = options.GetRefresher();
            });

            // Make settings and configuration refresher available through DI
            var configurationRoot = configurationBuilder.Build();

            if (configurationRefresher is null)
            {

#pragma warning disable CA1303 // Do not pass literals as localized parameters
                throw new InvalidOperationException($"{nameof(IConfigurationRefresher)} instance wasn't created");
#pragma warning restore CA1303 // Do not pass literals as localized parameters
            }

            var config = configurationRoot.GetSection("CatalogueScanner:Settings");
            builder.Services.Configure<CatalogueScannerSettings>(config);
            builder.Services.AddSingleton(configurationRefresher);
            #endregion
        }
    }
}