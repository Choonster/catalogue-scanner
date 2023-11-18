using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace CatalogueScanner.Configuration
{
    public static class ConfigurationBuilderExensions
    {
        public static IConfigurationBuilder AddCatalogueScannerAzureAppConfiguration(this IConfigurationBuilder configurationBuilder, string connectionString)
        {
            #region null checks
            ArgumentNullException.ThrowIfNull(configurationBuilder);

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException($"'{nameof(connectionString)}' cannot be null or empty.", nameof(connectionString));
            }
            #endregion

            configurationBuilder.AddAzureAppConfiguration(options =>
            {
                options.Connect(connectionString)
                       // Load all keys that start with `CatalogueScanner:`
                       .Select("CatalogueScanner:*")
                       // Configure to reload configuration if the registered 'Sentinel' key is modified
                       .ConfigureRefresh(refreshOptions =>
                            refreshOptions.Register(ConfigurationConstants.SentinelKey, refreshAll: true)
                        );
            });

            return configurationBuilder;
        }

        public static IServiceCollection SetAzureAppConfigurationConnectionString(this IServiceCollection services, string connectionString)
        {
            services.AddOptions<Options.AzureAppConfigurationOptions>()
                .Configure(options => options.ConnectionString = connectionString);

            return services;
        }
    }
}
