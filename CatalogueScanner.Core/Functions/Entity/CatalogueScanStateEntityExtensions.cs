using Microsoft.DurableTask.Client.Entities;
using Microsoft.DurableTask.Entities;

namespace CatalogueScanner.Core.Functions.Entity;

public static class CatalogueScanStateEntityExtensions
{
    public static async Task<ScanState> GetScanStateAsync(this TaskOrchestrationEntityFeature entities, EntityInstanceId scanStateId, CallEntityOptions? options = null)
    {
        #region null checks
        ArgumentNullException.ThrowIfNull(entities);
        #endregion

        return await entities.CallEntityAsync<ScanState>(scanStateId, nameof(CatalogueScanStateEntity.Get), options).ConfigureAwait(true);
    }

    public static async Task UpdateScanStateAsync(this TaskOrchestrationEntityFeature entities, EntityInstanceId scanStateId, ScanState scanState, CallEntityOptions? options = null)
    {
        #region null checks
        ArgumentNullException.ThrowIfNull(entities);
        #endregion

        await entities.CallEntityAsync(scanStateId, nameof(CatalogueScanStateEntity.Update), scanState, options).ConfigureAwait(true);
    }

    public static async Task SignalUpdateScanStateAsync(this DurableEntityClient entities, EntityInstanceId scanStateId, ScanState scanState, CancellationToken cancellationToken = default)
    {
        #region null checks
        ArgumentNullException.ThrowIfNull(entities);
        #endregion

        await entities.SignalEntityAsync(scanStateId, nameof(CatalogueScanStateEntity.Update), scanState, null, cancellationToken).ConfigureAwait(false);
    }
}
