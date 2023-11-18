using Microsoft.Extensions.Configuration;

namespace CatalogueScanner.Configuration
{
    public class TypedConfiguration<TOptions>(IConfigurationSection configuration) : ITypedConfiguration<TOptions> where TOptions : class, new()
    {
        public IConfigurationSection Configuration { get; } = configuration;
    }
}
