using CatalogueScanner.Core.Functions.Entity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CatalogueScanner.Core.Functions;

public static class CheckAndUpdateScanState
{
    /// <summary>
    /// Sub-orchestrator function that checks the current scan state of a catalogue and does the following:
    /// <list type="bullet">
    /// <item>
    /// If the state is <see cref="ScanState.NotStarted"/>, updates the state to <see cref="ScanState.InProgress"/> 
    /// and returns <c>true</c> to indicate that the parent orchestration should continue running
    /// </item>
    /// <item>
    /// If the state is any other value, returns <c>false</c> to indicate that the parent orchestration
    /// should stop immediately
    /// </item>
    /// </list>
    /// </summary>
    /// <returns><c>true</c> if the parent orchestration should continue running; otherwise, <c>false</c></returns>
    [Function(CoreFunctionNames.CheckAndUpdateScanState)]
    public static async Task<bool> RunOrchestrator([OrchestrationTrigger] TaskOrchestrationContext context, EntityInstanceId scanStateId)
    {
        #region null checks
        ArgumentNullException.ThrowIfNull(context);
        #endregion

        var logger = context.CreateReplaySafeLogger(typeof(CheckAndUpdateScanState));

        await using ((await context.Entities.LockEntitiesAsync(scanStateId).ConfigureAwait(true)).ConfigureAwait(true))
        {
            logger.CheckingState(scanStateId.Key);

            var state = await context.Entities.GetScanStateAsync(scanStateId).ConfigureAwait(true);
            if (state != ScanState.NotStarted)
            {
                logger.SkippingScan(scanStateId.Key, state);
                return false;
            }

            await context.Entities.UpdateScanStateAsync(scanStateId, ScanState.InProgress).ConfigureAwait(true);

            return true;
        }
    }
}
