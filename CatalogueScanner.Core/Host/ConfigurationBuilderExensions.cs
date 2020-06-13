using Microsoft.Extensions.Configuration;

namespace CatalogueScanner.Core.Host
{
    public static class ConfigurationBuilderExensions
    {
        public static IConfigurationBuilder AddCatalogueScannerAzureAppConfiguration(this IConfigurationBuilder configurationBuilder, string connectionString)
        {
            configurationBuilder.AddAzureAppConfiguration(options =>
            {
                options.Connect(connectionString)
                       // Load all keys that start with `CatalogueScanner:`
                       .Select("CatalogueScanner:*")
                       // Configure to reload configuration if the registered 'Sentinel' key is modified
                       .ConfigureRefresh(refreshOptions =>
                            refreshOptions.Register("CatalogueScanner:Sentinel", refreshAll: true)
                        );
            });

            return configurationBuilder;
        }
    }
}
