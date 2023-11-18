using CatalogueScanner.ColesOnline.Options;
using CatalogueScanner.ColesOnline.Service;
using CatalogueScanner.Configuration;
using CatalogueScanner.Core.Host;
using Microsoft.Extensions.DependencyInjection;

namespace CatalogueScanner.ColesOnline
{
    public class ColesOnlineCatalogueScannerPlugin : ICatalogueScannerPlugin
    {
        public void Configure(ICatalogueScannerHostBuilder builder)
        {
            #region null checks
            ArgumentNullException.ThrowIfNull(builder);
            #endregion

            builder.Services
                .AddHttpClient<ColesOnlineService>(client =>
                {
                    client.BaseAddress = new Uri("https://www.coles.com.au/");
                });

            AddConfiguration(builder);
        }

        private static void AddConfiguration(ICatalogueScannerHostBuilder builder)
        {
            var colesOnlineSection = builder.Configuration.GetSection("ColesOnline");

            builder.Services
                .ConfigureOptions<ColesOnlineOptions>(colesOnlineSection.GetSection(ColesOnlineOptions.ColesOnline));
        }
    }
}
