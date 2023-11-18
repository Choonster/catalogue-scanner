using CatalogueScanner.ColesOnline.Dto.FunctionInput;
using CatalogueScanner.Core;
using CatalogueScanner.Core.Dto.EntityKey;
using CatalogueScanner.Core.Dto.FunctionInput;
using CatalogueScanner.Core.Dto.FunctionResult;
using CatalogueScanner.Core.Functions.Entity;
using CatalogueScanner.Core.Utility;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace CatalogueScanner.ColesOnline.Functions;

public static class ScanColesOnlineSpecials
{
    private const string CatalogueType = "ColesOnline";
    private const string Store = "Coles Online";

    [Function(ColesOnlineFunctionNames.ScanColesOnlineSpecials)]
    public static async Task RunOrchestrator([OrchestrationTrigger] TaskOrchestrationContext context)
    {
        #region null checks
        ArgumentNullException.ThrowIfNull(context);
        #endregion

        var logger = context.CreateReplaySafeLogger(typeof(ScanColesOnlineSpecials));

        var specialsDateRange = await context.CallActivityAsync<DateRange>(ColesOnlineFunctionNames.GetColesOnlineSpecialsDates, null, null).ConfigureAwait(true);
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

            var buildId = await context.CallActivityAsync<string>(ColesOnlineFunctionNames.GetColesOnlineBuildId).ConfigureAwait(true);

            var taskOptions = new TaskOptions(new RetryPolicy(maxNumberOfAttempts: 5, firstRetryInterval: TimeSpan.FromSeconds(30)));

            var pageCount = await context.CallActivityAsync<int>(
                   ColesOnlineFunctionNames.GetColesOnlineSpecialsPageCount,
                   new GetColesOnlineSpecialsPageCountInput(buildId),
                   taskOptions
            ).ConfigureAwait(true);

            var downloadTasks = Enumerable.Range(0, pageCount)
                .Select(pageIndex =>
                    context.CallActivityAsync<IEnumerable<CatalogueItem>>(
                        ColesOnlineFunctionNames.DownloadColesOnlineSpecialsPage,
                        new DownloadColesOnlineSpecialsPageInput(buildId, pageIndex + 1),
                        taskOptions
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
    [Function(ColesOnlineFunctionNames.ScanColesOnlineSpecialsTimerStart)]
    public static async Task TimerStart(
        [TimerTrigger("%" + ColesOnlineAppSettingNames.ColesOnlineCronExpression + "%")] TimerInfo timer,
        [DurableClient] DurableTaskClient durableTaskClient,
        FunctionContext context,
        CancellationToken cancellationToken
    )
    {
        #region null checks
        ArgumentNullException.ThrowIfNull(timer);

        ArgumentNullException.ThrowIfNull(durableTaskClient);

        ArgumentNullException.ThrowIfNull(context);
        #endregion

        var logger = context.GetLogger(typeof(ScanColesOnlineSpecials).FullName!);

        var instanceId = await durableTaskClient.ScheduleNewOrchestrationInstanceAsync(
            ColesOnlineFunctionNames.ScanColesOnlineSpecials,
            cancellationToken
        ).ConfigureAwait(false);

        logger.LogInformation($"Started {ColesOnlineFunctionNames.ScanColesOnlineSpecials} orchestration with ID = '{{instanceId}}'.", instanceId);
    }
}
