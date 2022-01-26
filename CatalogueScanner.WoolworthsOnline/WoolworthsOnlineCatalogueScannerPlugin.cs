using CatalogueScanner.Configuration;
using CatalogueScanner.Core.Host;
using CatalogueScanner.WoolworthsOnline.Options;
using CatalogueScanner.WoolworthsOnline.Service;
using Microsoft.Extensions.DependencyInjection;

namespace CatalogueScanner.WoolworthsOnline
{
    public class WoolworthsOnlineCatalogueScannerPlugin : ICatalogueScannerPlugin
    {
        public void Configure(ICatalogueScannerHostBuilder builder)
        {
            #region null checks
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            #endregion

            builder.Services
                .AddHttpClient<WoolworthsOnlineService>(client =>
                {
                    client.BaseAddress = new Uri("https://www.woolworths.com.au/apis/ui");
                });

            AddConfiguration(builder);
        }

        private static void AddConfiguration(ICatalogueScannerHostBuilder builder)
        {
            var woolworthsOnlineSection = builder.Configuration.GetSection("WoolworthsOnline");

            builder.Services
                .ConfigureOptions<WoolworthsOnlineOptions>(woolworthsOnlineSection.GetSection(WoolworthsOnlineOptions.WoolworthsOnline));
        }
    }
}
