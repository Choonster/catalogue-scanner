using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CatalogueScanner.Core.Host
{
    public interface ICatalogueScannerHostBuilder
    {
        IServiceCollection Services { get; }
        IConfiguration Configuration { get; }
    }
}
