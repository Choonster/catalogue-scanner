using CatalogueScanner.Core;
using CatalogueScanner.Core.Dto.EntityKey;
using CatalogueScanner.Core.Dto.FunctionInput;
using CatalogueScanner.Core.Dto.FunctionResult;
using CatalogueScanner.Core.Functions.Entity;
using CatalogueScanner.Core.Utility;
using CatalogueScanner.WoolworthsOnline.Dto.FunctionInput;
using CatalogueScanner.WoolworthsOnline.Dto.FunctionResult;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CatalogueScanner.WoolworthsOnline.Functions
{
    public static class ScanWoolworthsOnlineSpecials
    {
        private const string CatalogueType = "WoolworthsOnline";
        private const string Store = "Woolworths";

        [Function(WoolworthsOnlineFunctionNames.ScanWoolworthsOnlineSpecials)]
        public static async Task RunOrchestrator([OrchestrationTrigger] TaskOrchestrationContext context)
        {
            #region null checks
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            #endregion

            var logger = context.CreateReplaySafeLogger(typeof(ScanWoolworthsOnlineSpecials));

            var specialsDateRange = await context.CallActivityAsync<DateRange>(WoolworthsOnlineFunctionNames.GetWoolworthsOnlineSpecialsDates).ConfigureAwait(true);
            var dateKey = $"Start={specialsDateRange.StartDate:O};End={specialsDateRange.EndDate:O}";

            var scanStateId = CatalogueScanStateEntity.CreateId(new CatalogueScanStateKey(CatalogueType, Store, dateKey));

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
                logger.LogDebug($"Downloading - {scanStateId.Key}");

                var categories = await context.CallActivityAsync<IEnumerable<WoolworthsOnlineCategory>>(
                    WoolworthsOnlineFunctionNames.GetWoolworthsOnlineSpecialsCategories
                ).ConfigureAwait(true);

                var retryOptions = new RetryPolicy(firstRetryInterval: TimeSpan.FromSeconds(30), maxNumberOfAttempts: 5);
                var taskOptions = new TaskOptions(retryOptions);

                var itemPages = new List<IEnumerable<CatalogueItem>>();

                foreach (var category in categories)
                {
                    var pageCount = await context.CallActivityAsync<int>(
                        WoolworthsOnlineFunctionNames.GetWoolworthsOnlineSpecialsPageCount,
                        category.CategoryId,
                        taskOptions
                    ).ConfigureAwait(true);

                    var downloadTasks = Enumerable.Range(0, pageCount)
                        .Select(pageIndex =>
                            context.CallActivityAsync<IEnumerable<CatalogueItem>>(
                                WoolworthsOnlineFunctionNames.DownloadWoolworthsOnlineSpecialsPage,
                                new DownloadWoolworthsOnlineSpecialsPageInput
                                {
                                    CategoryId = category.CategoryId,
                                    PageNumber = pageIndex + 1,
                                },
                                taskOptions
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
                logger.LogDebug($"Updating state - {scanStateId.Key}");

                await context.Entities.UpdateScanStateAsync(scanStateId, ScanState.Completed).ConfigureAwait(true);
                #endregion

                logger.LogDebug($"Completed - {scanStateId.Key}");
                context.SetCustomStatus("Completed");
            }
            catch
            {
                await context.Entities.UpdateScanStateAsync(scanStateId, ScanState.Failed).ConfigureAwait(true);
                context.SetCustomStatus("Failed");

                throw;
            }
        }

        /// <summary>
        /// Function that triggers the <see cref="RunOrchestrator"/> orchestrator function on a timer.
        /// </summary>
        [Function(WoolworthsOnlineFunctionNames.ScanWoolworthsOnlineSpecialsTimerStart)]
        public static async Task TimerStart(
            [TimerTrigger("%" + WoolworthsOnlineAppSettingNames.WoolworthsOnlineCronExpression + "%")] TimerInfo timer,
            [DurableClient] DurableTaskClient durableTaskClient,
            FunctionContext context,
            CancellationToken cancellationToken
        )
        {
            #region null checks
            if (timer is null)
            {
                throw new ArgumentNullException(nameof(timer));
            }

            if (durableTaskClient is null)
            {
                throw new ArgumentNullException(nameof(durableTaskClient));
            }

            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            #endregion

            var logger = context.GetLogger(typeof(ScanWoolworthsOnlineSpecials).FullName!);

            var instanceId = await durableTaskClient.ScheduleNewOrchestrationInstanceAsync(
                WoolworthsOnlineFunctionNames.ScanWoolworthsOnlineSpecials,
                cancellationToken
            ).ConfigureAwait(false);

            logger.LogInformation($"Started {WoolworthsOnlineFunctionNames.ScanWoolworthsOnlineSpecials} orchestration with ID = '{{instanceId}}'.", instanceId);
        }
    }
}
