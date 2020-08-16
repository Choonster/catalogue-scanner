using Microsoft.Extensions.Configuration;

namespace CatalogueScanner.Configuration
{
    public class TypedConfiguration<TOptions> : ITypedConfiguration<TOptions> where TOptions : class, new()
    {
        public TypedConfiguration(IConfigurationSection configuration)
        {
            Configuration = configuration;
        }

        public IConfigurationSection Configuration { get; }
    }
}
