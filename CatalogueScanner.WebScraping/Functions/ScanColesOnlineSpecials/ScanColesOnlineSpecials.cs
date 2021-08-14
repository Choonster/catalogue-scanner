using CatalogueScanner.Core;
using CatalogueScanner.Core.Dto.EntityKey;
using CatalogueScanner.Core.Dto.FunctionResult;
using CatalogueScanner.Core.Functions.Entity;
using CatalogueScanner.Core.Utility;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CatalogueScanner.WebScraping.Functions
{
    public static class ScanColesOnlineSpecials
    {
        private const string CatalogueType = "ColesOnline";
        private const string Store = "Coles";

        [FunctionName(WebScrapingFunctionNames.ScanColesOnlineSpecials)]
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

            var specialsDateRange = await context.CallActivityAsync<DateRange>(WebScrapingFunctionNames.GetColesOnlineSpecialsDates, null).ConfigureAwait(true);
            var dateKey = $"Start={specialsDateRange.StartDate:O};End={specialsDateRange.EndDate:O}";

            var scanStateId = ICatalogueScanState.CreateId(new CatalogueScanStateKey(CatalogueType, Store, dateKey));
            var scanState = context.CreateEntityProxy<ICatalogueScanState>(scanStateId);

            using (await context.LockAsync(scanStateId).ConfigureAwait(true))
            {
                #region Check and update the catalogue's scan state
                context.SetCustomStatus("CheckingState");
                log.LogDebug($"Checking state - {scanStateId.EntityKey}");

                var state = await scanState.GetState().ConfigureAwait(true);
                if (state != ScanState.NotStarted)
                {
                    log.LogInformation($"Catalogue {scanStateId.EntityKey} already in state {state}, skipping scan.");
                    context.SetCustomStatus("Skipped");
                    return;
                }

                await scanState.UpdateState(ScanState.InProgress).ConfigureAwait(true);
                #endregion

                #region Download catalogue
                context.SetCustomStatus("Downloading");
                log.LogDebug($"Downloading - {scanStateId.EntityKey}");

                var downloadedCatalogue = await context.CallActivityAsync<Catalogue>(WebScrapingFunctionNames.DownloadColesOnlineSpecials, specialsDateRange).ConfigureAwait(true);
                #endregion

                #region Filter catalouge items
                context.SetCustomStatus("Filtering");
                log.LogDebug($"Filtering - {scanStateId.EntityKey}");

                var itemTasks = downloadedCatalogue.Items
                    .Select(item => context.CallActivityAsync<CatalogueItem?>(CoreFunctionNames.FilterCatalogueItem, item))
                    .ToList();

                await Task.WhenAll(itemTasks).ConfigureAwait(true);
                #endregion

                #region Send digest email
                context.SetCustomStatus("SendingDigestEmail");
                log.LogDebug($"Sending digest email - {scanStateId.EntityKey}");

                var filteredItems = itemTasks
                    .Where(task => task.Result != null)
                    .Select(task => task.Result!)
                    .ToList();

                if (filteredItems.Any())
                {
                    var filteredCatalogue = new Catalogue(downloadedCatalogue.Store, downloadedCatalogue.StartDate, downloadedCatalogue.EndDate, filteredItems);

                    await context.CallActivityAsync(CoreFunctionNames.SendCatalogueDigestEmail, filteredCatalogue).ConfigureAwait(true);
                }
                else
                {
                    log.LogInformation($"Catalogue {scanStateId.EntityKey} had no matching items, skipping digest email.");
                }
                #endregion

                #region Update catalogue's scan state
                context.SetCustomStatus("UpdatingState");
                log.LogDebug($"Updating state - {scanStateId.EntityKey}");

                await scanState.UpdateState(ScanState.Completed).ConfigureAwait(true);
                #endregion

                log.LogDebug($"Completed - {scanStateId.EntityKey}");
                context.SetCustomStatus("Completed");
            }
        }

        /// <summary>
        /// Function that triggers the <see cref="RunOrchestrator"/> orchestrator function on a timer.
        /// </summary>
        [FunctionName(WebScrapingFunctionNames.ScanColesOnlineSpecialsTimerStart)]
        public static async Task TimerStart(
            [TimerTrigger("%" + WebScrapingAppSettingNames.ColesOnlineCronExpression +"%")] TimerInfo timer,
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

            var instanceId = await starter.StartNewAsync(WebScrapingFunctionNames.ScanColesOnlineSpecials, null).ConfigureAwait(false);

            log.LogInformation($"Started {WebScrapingFunctionNames.ScanColesOnlineSpecials} orchestration with ID = '{{instanceId}}'.", instanceId);
        }
    }
}
