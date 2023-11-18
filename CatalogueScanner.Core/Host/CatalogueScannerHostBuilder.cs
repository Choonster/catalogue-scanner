using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace CatalogueScanner.Core.Host
{
    public class CatalogueScannerHostBuilder : ICatalogueScannerHostBuilder
    {
        public CatalogueScannerHostBuilder(IConfiguration configuration, IServiceCollection services)
        {
            #region null checks
            ArgumentNullException.ThrowIfNull(configuration);
            #endregion

            Configuration = configuration.GetSection("CatalogueScanner");
            Services = services ?? throw new ArgumentNullException(nameof(services));
        }

        public IConfiguration Configuration { get; }
        public IServiceCollection Services { get; }
    }
}
