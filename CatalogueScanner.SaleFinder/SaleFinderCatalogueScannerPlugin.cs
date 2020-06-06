using CatalogueScanner.Core.Host;
using CatalogueScanner.SaleFinder.Options;
using CatalogueScanner.SaleFinder.Service;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace CatalogueScanner.SaleFinder
{
    public class SaleFinderCatalogueScannerPlugin : ICatalogueScannerPlugin
    {
        public void Configure(ICatalogueScannerHostBuilder builder)
        {
            #region null checks
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            #endregion

            builder.Services.AddHttpClient();
            builder.Services.AddHttpClient<SaleFinderService>();

            AddConfiguration(builder);
        }

        private static void AddConfiguration(ICatalogueScannerHostBuilder builder)
        {
            var saleFinderSection = builder.Configuration.GetSection("SaleFinder");

            builder.Services
                .Configure<ColesOptions>(saleFinderSection.GetSection(ColesOptions.Coles))
                .Configure<WoolworthsOptions>(saleFinderSection.GetSection(WoolworthsOptions.Woolworths));
        }
    }
}
