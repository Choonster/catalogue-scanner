using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace CatalogueScanner.Configuration
{
    public static class ConfigurationBuilderExensions
    {
        public static IConfigurationBuilder AddCatalogueScannerAzureAppConfiguration(this IConfigurationBuilder configurationBuilder, string connectionString, out Func<IConfigurationRefresher> refresherSupplier)
        {
            IConfigurationRefresher? refresher = null;

            configurationBuilder.AddAzureAppConfiguration(options =>
            {
                options.Connect(connectionString)
                       // Load all keys that start with `CatalogueScanner:`
                       .Select("CatalogueScanner:*")
                       // Configure to reload configuration if the registered 'Sentinel' key is modified
                       .ConfigureRefresh(refreshOptions =>
                            refreshOptions.Register(ConfigurationConstants.SentinelKey, refreshAll: true)
                        );

                refresher = options.GetRefresher();
            });

            refresherSupplier = () =>
            {
                if (refresher is null)
                {
                    throw new InvalidOperationException($"{nameof(IConfigurationRefresher)} hasn't been created yet");
                }
                return refresher;
            };

            return configurationBuilder;
        }

        public static IServiceCollection SetAzureAppConfigurationConnectionString(this IServiceCollection services, string connectionString)
        {
            services.AddOptions<Options.AzureAppConfigurationOptions>()
                .Configure(options => options.ConnectionString = connectionString);

            return services;
        }

        public static IServiceCollection SetConfigurationRefresher(this IServiceCollection services, Func<IConfigurationRefresher> refresherSupplier)
        {
            services.AddSingleton(refresherSupplier.Invoke());

            return services;
        }
    }
}
