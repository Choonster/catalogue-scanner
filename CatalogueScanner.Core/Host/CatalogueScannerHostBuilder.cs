using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace CatalogueScanner.Core.Host
{
    public class CatalogueScannerHostBuilder : ICatalogueScannerHostBuilder
    {
        public CatalogueScannerHostBuilder(IFunctionsHostBuilder functionsHostBuilder, IConfiguration configuration)
        {
            #region null checks
            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            } 
            #endregion

            FunctionsHostBuilder = functionsHostBuilder ?? throw new ArgumentNullException(nameof(functionsHostBuilder));
            Services = functionsHostBuilder.Services;

            Configuration = configuration.GetSection("CatalogueScanner");
        }

        public IFunctionsHostBuilder FunctionsHostBuilder { get; }

        public IConfiguration Configuration { get; }

        public IServiceCollection Services { get; }
    }
}
