using CatalogueScanner.Core;
using CatalogueScanner.Core.Dto.EntityKey;
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
                log.LogInformation("Entering lock for {ScanStateId}", scanStateId);

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

                    var categories = await context.CallActivityAsync<IEnumerable<WoolworthsOnlineCategory>>(
                        WoolworthsOnlineFunctionNames.GetWoolworthsOnlineSpecialsCategories,
                        null
                    ).ConfigureAwait(true);

                    var retryOptions = new RetryOptions(firstRetryInterval: TimeSpan.FromSeconds(30), maxNumberOfAttempts: 5)
                    {
                        BackoffCoefficient = 1.1,
                    };

                    var categoryPagesCounts = await Task.WhenAll(
                        categories.Select(async (category) => (
                            category.CategoryId,
                            PageCount: await context.CallActivityWithRetryAsync<int>(
                                WoolworthsOnlineFunctionNames.GetWoolworthsOnlineSpecialsPageCount,
                                retryOptions,
                                category.CategoryId
                            ).ConfigureAwait(true)
                        ))
                    ).ConfigureAwait(true);

                    var inputs = categoryPagesCounts
                        .Select(category =>
                            Enumerable.Range(0, category.PageCount)
                                      .Select(pageIndex => new DownloadWoolworthsOnlineSpecialsPageInput
                                      {
                                          CategoryId = category.CategoryId,
                                          PageNumber = pageIndex + 1,
                                      })
                        )
                        .SelectMany(category => category)
                        .ToList();

                    var itemPages = await Throttler.ExecuteInBatches(
                        inputs,
                        5,
                        input => context.CallActivityWithRetryAsync<IEnumerable<CatalogueItem>>(
                                    WoolworthsOnlineFunctionNames.DownloadWoolworthsOnlineSpecialsPage,
                                    retryOptions,
                                    input
                                 )
                    ).ConfigureAwait(true);
                    #endregion

                    #region Filter catalouge items
                    context.SetCustomStatus("Filtering");
                    log.LogDebug($"Filtering - {scanStateId.EntityKey}");

                    var itemTasks = itemPages
                        .SelectMany(page => page)
                        .Select(item => context.CallActivityAsync<CatalogueItem?>(CoreFunctionNames.FilterCatalogueItem, item))
                        .ToList();

                    var filterResults = await Task.WhenAll(itemTasks).ConfigureAwait(true);
                    #endregion

                    #region Send digest email
                    context.SetCustomStatus("SendingDigestEmail");
                    log.LogDebug($"Sending digest email - {scanStateId.EntityKey}");

                    var filteredItems = filterResults
                        .Where(item => item != null)
                        .Select(item => item!)
                        .ToList();

                    log.LogDebug("{NumItems} items remain after filtering", filteredItems.Count);

                    if (filteredItems.Any())
                    {
                        var filteredCatalogue = new Catalogue("Woolworths Online", specialsDateRange.StartDate, specialsDateRange.EndDate, CurrencyCultures.AustralianDollar, filteredItems);

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
