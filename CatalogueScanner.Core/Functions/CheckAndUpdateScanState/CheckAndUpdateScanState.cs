using CatalogueScanner.Core.Functions.Entity;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CatalogueScanner.Core.Functions
{
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
        [FunctionName(CoreFunctionNames.CheckAndUpdateScanState)]
        public static async Task<bool> RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            #region null checks
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (log is null)
            {
                throw new ArgumentNullException(nameof(log));
            }
            #endregion

            var scanStateId = context.GetInput<EntityId>();
            var scanState = context.CreateEntityProxy<ICatalogueScanState>(scanStateId);

            using (await context.LockAsync(scanStateId).ConfigureAwait(true))
            {
                log.LogDebug($"Checking state - {scanStateId.EntityKey}");

                var state = await scanState.GetState().ConfigureAwait(true);
                if (state != ScanState.NotStarted)
                {
                    log.LogInformation($"Catalogue {scanStateId.EntityKey} already in state {state}, skipping scan.");
                    return false;
                }

                await scanState.UpdateState(ScanState.InProgress).ConfigureAwait(true);

                return true;
            }
        }
    }
}
