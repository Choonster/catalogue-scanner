using CatalogueScanner.Core.Dto.EntityKey;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Entities;

namespace CatalogueScanner.Core.Functions.Entity;

/// <summary>
/// Durable entity that stores the scan state of an individual catalogue, so that each catalogue is only scanned once.
/// </summary>
public class CatalogueScanStateEntity : TaskEntity<ScanState>
{
    /// <summary>
    /// Creates an <see cref="EntityInstanceId"/> for a catalogue scan state entity.
    /// </summary>
    /// <param name="key">The catalogue scan state key</param>
    /// <returns>The entity instance ID</returns>
    public static EntityInstanceId CreateId(CatalogueScanStateKey key)
    {
        #region null checks
        ArgumentNullException.ThrowIfNull(key);
        #endregion

        return new EntityInstanceId(CoreFunctionNames.CatalogueScanState, key.ToString());
    }

    /// <returns>The current scan state of the catalogue</returns>
    public ScanState Get() => State;

    /// <summary>
    /// Updates the current scan state of the catalogue
    /// </summary>
    /// <param name="scanState">The new scan state</param>
    public void Update(ScanState scanState) => State = scanState;

    [Function(CoreFunctionNames.CatalogueScanState)]
    public Task DispatchAsync([EntityTrigger] TaskEntityDispatcher dispatcher)
    {
        #region null checks
        ArgumentNullException.ThrowIfNull(dispatcher);
        #endregion

        return dispatcher.DispatchAsync(this);
    }
}
