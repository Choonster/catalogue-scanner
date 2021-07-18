using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace CatalogueScanner.Core.Host
{
    /// <summary>
    /// Dummy implementation of <see cref="IFunctionsHostBuilder"/> that wraps an existing <see cref="IServiceCollection"/>.
    /// </summary>
    public class DummyFunctionsHostBuilder : IFunctionsHostBuilder
    {
        public IServiceCollection Services { get; }

        public DummyFunctionsHostBuilder(IServiceCollection services)
        {
            Services = services;
        }
    }
}
