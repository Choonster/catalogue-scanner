using System.Threading.Tasks;

namespace CatalogueScanner.Core.Entity
{
    /// <summary>
    /// Durable entity that stores the scan state of an individual catalogue, so that each catalogue is only scanned once.
    /// </summary>
    public interface ICatalogueScanState
    {
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
