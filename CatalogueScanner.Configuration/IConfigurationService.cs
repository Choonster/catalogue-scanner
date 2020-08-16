using System.Threading;
using System.Threading.Tasks;

namespace CatalogueScanner.Configuration
{
    public interface IConfigurationService<TOptions>
    {
        Task SaveAsync(TOptions options, CancellationToken cancellationToken = default);
    }
}