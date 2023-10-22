using CatalogueScanner.ColesOnline.Dto.FunctionInput;
using CatalogueScanner.Core;
using CatalogueScanner.Core.Dto.EntityKey;
using CatalogueScanner.Core.Dto.FunctionInput;
using CatalogueScanner.Core.Dto.FunctionResult;
using CatalogueScanner.Core.Functions.Entity;
using CatalogueScanner.Core.Utility;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace CatalogueScanner.ColesOnline.Functions
{
    public static class ScanColesOnlineSpecials
    {
        private const string CatalogueType = "ColesOnline";
        private const string Store = "Coles Online";

        [FunctionName(ColesOnlineFunctionNames.ScanColesOnlineSpecials)]
        public static async Task RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
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

            log = context.CreateReplaySafeLogger(log);

            var specialsDateRange = await context.CallActivityAsync<DateRange>(ColesOnlineFunctionNames.GetColesOnlineSpecialsDates, null).ConfigureAwait(true);
            var dateKey = $"Start={specialsDateRange.StartDate:O};End={specialsDateRange.EndDate:O}";

            var scanStateId = ICatalogueScanState.CreateId(new CatalogueScanStateKey(CatalogueType, Store, dateKey));
            var scanState = context.CreateEntityProxy<ICatalogueScanState>(scanStateId);

            try
            {
                #region Check and update the catalogue's scan state
                context.SetCustomStatus("CheckingState");

                var shouldContinue = await context.CallSubOrchestratorAsync<bool>(CoreFunctionNames.CheckAndUpdateScanState, scanStateId).ConfigureAwait(true);

                if (!shouldContinue)
                {
                    context.SetCustomStatus("Skipped");
                    return;
                }
                #endregion

                #region Download catalogue
                context.SetCustomStatus("Downloading");
                log.LogDebug($"Downloading - {scanStateId.EntityKey}");

                var buildId = await context.CallActivityAsync<string>(ColesOnlineFunctionNames.GetColesOnlineBuildId, null).ConfigureAwait(true);

                var retryOptions = new RetryOptions(firstRetryInterval: TimeSpan.FromSeconds(30), maxNumberOfAttempts: 5);

                var pageCount = await context.CallActivityWithRetryAsync<int>(
                       ColesOnlineFunctionNames.GetColesOnlineSpecialsPageCount,
                       retryOptions,
                       new GetColesOnlineSpecialsPageCountInput(buildId)
                ).ConfigureAwait(true);

                var downloadTasks = Enumerable.Range(0, pageCount)
                    .Select(pageIndex =>
                        context.CallActivityWithRetryAsync<IEnumerable<CatalogueItem>>(
                            ColesOnlineFunctionNames.DownloadColesOnlineSpecialsPage,
                            retryOptions,
                            new DownloadColesOnlineSpecialsPageInput(buildId, pageIndex + 1)
                        )
                    )
                    .ToList();

                var itemPages = await Task.WhenAll(downloadTasks).ConfigureAwait(true);

                var items = itemPages
                    .SelectMany(page => page)
                    .ToList();

                var catalogue = new Catalogue(Store, specialsDateRange.StartDate, specialsDateRange.EndDate, CurrencyCultures.AustralianDollar, items);
                #endregion

                #region Filter catalouge items and send digest email
                context.SetCustomStatus("FilteringAndSendingDigestEmail");

                await context.CallSubOrchestratorAsync(
                    CoreFunctionNames.FilterCatalogueAndSendDigestEmail,
                    new FilterCatalogueAndSendDigestEmailInput(catalogue, scanStateId)
                ).ConfigureAwait(true);
                #endregion

                #region Update catalogue's scan state
                context.SetCustomStatus("UpdatingState");
                log.LogDebug($"Updating state - {scanStateId.EntityKey}");

                await scanState.UpdateState(ScanState.Completed).ConfigureAwait(true);
                #endregion

                log.LogDebug($"Completed - {scanStateId.EntityKey}");
                context.SetCustomStatus("Completed");
            }
            catch
            {
                await scanState.UpdateState(ScanState.Failed).ConfigureAwait(true);
                context.SetCustomStatus("Failed");

                throw;
            }
        }

        /// <summary>
        /// Function that triggers the <see cref="RunOrchestrator"/> orchestrator function on a timer.
        /// </summary>
        [FunctionName(ColesOnlineFunctionNames.ScanColesOnlineSpecialsTimerStart)]
        public static async Task TimerStart(
            [TimerTrigger("%" + ColesOnlineAppSettingNames.ColesOnlineCronExpression + "%")] TimerInfo timer,
            [DurableClient] IDurableClient starter,
            ILogger log
        )
        {
            #region null checks
            if (timer is null)
            {
                throw new ArgumentNullException(nameof(timer));
            }

            if (starter is null)
            {
                throw new ArgumentNullException(nameof(starter));
            }

            if (log is null)
            {
                throw new ArgumentNullException(nameof(log));
            }
            #endregion

            var instanceId = await starter.StartNewAsync(ColesOnlineFunctionNames.ScanColesOnlineSpecials, null).ConfigureAwait(false);

            log.LogInformation($"Started {ColesOnlineFunctionNames.ScanColesOnlineSpecials} orchestration with ID = '{{instanceId}}'.", instanceId);
        }
    }
}
