using CatalogueScanner.Core;
using CatalogueScanner.Core.Dto.EntityKey;
using CatalogueScanner.Core.Dto.FunctionInput;
using CatalogueScanner.Core.Dto.FunctionResult;
using CatalogueScanner.Core.Functions.Entity;
using CatalogueScanner.Core.Utility;
using CatalogueScanner.WoolworthsOnline.Dto.FunctionInput;
using CatalogueScanner.WoolworthsOnline.Dto.FunctionResult;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CatalogueScanner.WoolworthsOnline.Functions
{
    public static class ScanWoolworthsOnlineSpecials
    {
        private const string CatalogueType = "WoolworthsOnline";
        private const string Store = "Woolworths";

        [FunctionName(WoolworthsOnlineFunctionNames.ScanWoolworthsOnlineSpecials)]
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

            var specialsDateRange = await context.CallActivityAsync<DateRange>(WoolworthsOnlineFunctionNames.GetWoolworthsOnlineSpecialsDates, null).ConfigureAwait(true);
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

                var categories = await context.CallActivityAsync<IEnumerable<WoolworthsOnlineCategory>>(
                    WoolworthsOnlineFunctionNames.GetWoolworthsOnlineSpecialsCategories,
                    null
                ).ConfigureAwait(true);

                var retryOptions = new RetryOptions(firstRetryInterval: TimeSpan.FromSeconds(30), maxNumberOfAttempts: 5);

                var itemPages = new List<IEnumerable<CatalogueItem>>();

                foreach (var category in categories)
                {
                    var pageCount = await context.CallActivityWithRetryAsync<int>(
                        WoolworthsOnlineFunctionNames.GetWoolworthsOnlineSpecialsPageCount,
                        retryOptions,
                        category.CategoryId
                    ).ConfigureAwait(true);

                    var downloadTasks = Enumerable.Range(0, pageCount)
                        .Select(pageIndex =>
                            context.CallActivityWithRetryAsync<IEnumerable<CatalogueItem>>(
                                WoolworthsOnlineFunctionNames.DownloadWoolworthsOnlineSpecialsPage,
                                retryOptions,
                                new DownloadWoolworthsOnlineSpecialsPageInput
                                {
                                    CategoryId = category.CategoryId,
                                    PageNumber = pageIndex + 1,
                                }
                            )
                        )
                        .ToList();

                    var pages = await Task.WhenAll(downloadTasks).ConfigureAwait(true);

                    itemPages.AddRange(pages);
                }

                var items = itemPages
                    .SelectMany(page => page)
                    .ToList();

                var catalogue = new Catalogue("Woolworths Online", specialsDateRange.StartDate, specialsDateRange.EndDate, CurrencyCultures.AustralianDollar, items);
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
        [FunctionName(WoolworthsOnlineFunctionNames.ScanWoolworthsOnlineSpecialsTimerStart)]
        public static async Task TimerStart(
            [TimerTrigger("%" + WoolworthsOnlineAppSettingNames.WoolworthsOnlineCronExpression + "%")] TimerInfo timer,
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

            var instanceId = await starter.StartNewAsync(WoolworthsOnlineFunctionNames.ScanWoolworthsOnlineSpecials, null).ConfigureAwait(false);

            log.LogInformation($"Started {WoolworthsOnlineFunctionNames.ScanWoolworthsOnlineSpecials} orchestration with ID = '{{instanceId}}'.", instanceId);
        }
    }
}
