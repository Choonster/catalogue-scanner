using CatalogueScanner.Dto.Config;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using System;


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
            IConfigurationRoot configurationRoot = configurationBuilder.Build();

            if (configurationRefresher is null)
            {
                throw new InvalidOperationException($"{nameof(IConfigurationRefresher)} instance wasn't created");
            }

            IConfigurationSection config = configurationRoot.GetSection("CatalogueScanner:Settings");
            builder.Services.Configure<CatalogueScannerSettings>(config);
            builder.Services.AddSingleton(configurationRefresher);
        }
    }
}