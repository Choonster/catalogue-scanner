using CatalogueScanner.Core.Host;
using CatalogueScanner.SaleFinder.Service;
using Microsoft.Extensions.DependencyInjection;

namespace CatalogueScanner.SaleFinder
{
    public class SaleFinderCatalogueScannerPlugin : ICatalogueScannerPlugin
    {
        public void Register(ICatalogueScannerHostBuilder builder)
        {
            builder.Services.AddHttpClient();
            builder.Services.AddHttpClient<SaleFinderService>();
        }
    }
}
