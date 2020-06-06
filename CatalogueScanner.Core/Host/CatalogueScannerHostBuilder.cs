using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace CatalogueScanner.Core.Host
{
    public class CatalogueScannerHostBuilder : ICatalogueScannerHostBuilder
    {
        private readonly IConfigurationRoot configurationRoot;

        public CatalogueScannerHostBuilder(IFunctionsHostBuilder functionsHostBuilder)
        {
            FunctionsHostBuilder = functionsHostBuilder;
            Services = functionsHostBuilder.Services;
            
            configurationRoot = BuildConfigurationRoot(Services);
            Configuration = configurationRoot.GetSection("CatalogueScanner");
        }

        public IFunctionsHostBuilder FunctionsHostBuilder { get; }

        public IConfiguration Configuration { get; }

        public IServiceCollection Services { get; }

        private IConfigurationRoot BuildConfigurationRoot(IServiceCollection services)
        {
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

            // Make configuration refresher available through DI
            var configurationRoot = configurationBuilder.Build();

            if (configurationRefresher is null)
            {

#pragma warning disable CA1303 // Do not pass literals as localized parameters
                throw new InvalidOperationException($"{nameof(IConfigurationRefresher)} instance wasn't created");
#pragma warning restore CA1303 // Do not pass literals as localized parameters
            }

            services.AddSingleton(configurationRefresher);

            return configurationRoot;
        }
    }
}
