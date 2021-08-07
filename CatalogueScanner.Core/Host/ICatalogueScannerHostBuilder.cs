using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CatalogueScanner.Core.Host
{
    public interface ICatalogueScannerHostBuilder
    {
        IFunctionsHostBuilder FunctionsHostBuilder { get; }
        IServiceCollection Services { get; }
        IConfiguration Configuration { get; }
        IConfiguration LocalConfiguration { get; }
    }
}
