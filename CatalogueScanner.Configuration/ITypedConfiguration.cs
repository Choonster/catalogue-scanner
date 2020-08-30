using Microsoft.Extensions.Configuration;

namespace CatalogueScanner.Configuration
{
    public interface ITypedConfiguration<TOptions>
    {
        IConfigurationSection Configuration { get; }
    }
}
