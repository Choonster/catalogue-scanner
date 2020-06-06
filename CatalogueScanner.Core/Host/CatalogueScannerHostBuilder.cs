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
            FunctionsHostBuilder = functionsHostBuilder ?? throw new ArgumentNullException(nameof(functionsHostBuilder));
            Services = functionsHostBuilder.Services;

            (var configurationRoot, var configurationRefresher) = BuildConfiguration();

            this.configurationRoot = configurationRoot;
            Services.AddSingleton(configurationRefresher);

            Configuration = configurationRoot.GetSection("CatalogueScanner");
        }

        public IFunctionsHostBuilder FunctionsHostBuilder { get; }

        public IConfiguration Configuration { get; }

        public IServiceCollection Services { get; }

        private static (IConfigurationRoot, IConfigurationRefresher) BuildConfiguration()
        {
            IConfigurationRefresher? configurationRefresher = null;

            // Load configuration from Azure App Configuration
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddAzureAppConfiguration(options =>
            {
                options.Connect(Environment.GetEnvironmentVariable(CoreAppSettingNames.AzureAppConfigurationConnectionString))
                       // Load all keys that start with `CatalogueScanner:`
                       .Select("CatalogueScanner:*")
                       // Configure to reload configuration if the registered 'Sentinel' key is modified
                       .ConfigureRefresh(refreshOptions =>
                            refreshOptions.Register("CatalogueScanner:Sentinel", refreshAll: true)
                        );

                configurationRefresher = options.GetRefresher();
            });

            var configurationRoot = configurationBuilder.Build();

            if (configurationRefresher is null)
            {

#pragma warning disable CA1303 // Do not pass literals as localized parameters
                throw new InvalidOperationException($"{nameof(IConfigurationRefresher)} instance wasn't created");
#pragma warning restore CA1303 // Do not pass literals as localized parameters
            }

            return (configurationRoot, configurationRefresher);
        }
    }
}
