using CatalogueScanner.Core.Dto.EntityKey;
using CatalogueScanner.Core.Functions.Entity.Implementation;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System.Threading.Tasks;

namespace CatalogueScanner.Core.Functions.Entity
{
    /// <summary>
    /// Durable entity that stores the scan state of an individual catalogue, so that each catalogue is only scanned once.
    /// </summary>
    public interface ICatalogueScanState
    {
        /// <summary>
        /// The name of the entity type.
        /// </summary>
        const string EntityName = nameof(CatalogueScanState);

        /// <summary>
        /// Creates an <see cref="EntityId"/> for a catalogue scan state entity.
        /// </summary>
        /// <param name="key">The catalogue scan state key</param>
        /// <returns>The entity ID</returns>
        static EntityId CreateId(CatalogueScanStateKey key) => new EntityId(EntityName, key.ToString());


        /// <summary>
        /// Entity operation that returns the current scan state of the catalogue.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the scan state.</returns>
        Task<ScanState> GetState();

        /// <summary>
        /// Entity operation that updates the scan state of the catalogue.
        /// </summary>
        /// <param name="scanState">The new scan state</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task UpdateState(ScanState scanState);
    }
}
