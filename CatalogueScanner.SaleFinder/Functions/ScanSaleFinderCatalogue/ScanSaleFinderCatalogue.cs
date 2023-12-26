using CatalogueScanner.Core;
using CatalogueScanner.Core.Dto.EntityKey;
using CatalogueScanner.Core.Dto.FunctionInput;
using CatalogueScanner.Core.Dto.FunctionResult;
using CatalogueScanner.Core.Functions.Entity;
using CatalogueScanner.SaleFinder.Dto.FunctionInput;
using CatalogueScanner.SaleFinder.Dto.FunctionResult;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CatalogueScanner.SaleFinder.Functions;

public static class ScanSaleFinderCatalogue
{
    private const string CatalogueType = "SaleFinder";

    /// <summary>
    /// Orchestrator function that downloads a SaleFinder catalogue, filters the items using the configured rules and sends an email digest.
    /// </summary>
    [Function(SaleFinderFunctionNames.ScanSaleFinderCatalogue)]
    public static async Task RunOrchestrator([OrchestrationTrigger] TaskOrchestrationContext context, SaleFinderCatalogueDownloadInformation catalogueDownloadInfo)
    {
        #region null checks
        ArgumentNullException.ThrowIfNull(context);

        ArgumentNullException.ThrowIfNull(catalogueDownloadInfo);
        #endregion

        var log = context.CreateReplaySafeLogger(typeof(ScanSaleFinderCatalogue));

        var scanStateId = CatalogueScanStateEntity.CreateId(new CatalogueScanStateKey(
            CatalogueType,
            catalogueDownloadInfo.Store,
            catalogueDownloadInfo.SaleId.ToString(CultureInfo.InvariantCulture)
        ));

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
            log.Downloading(scanStateId.Key);

            var downloadedCatalogue = await context.CallActivityAsync<Catalogue>(SaleFinderFunctionNames.DownloadSaleFinderCatalogue, catalogueDownloadInfo).ConfigureAwait(true);
            #endregion

            #region Fill prices
            context.SetCustomStatus("FillingPrices");
            log.FillingPrices(scanStateId.Key);

            var itemsWithPrices = await Task.WhenAll(
                downloadedCatalogue.Items.Select(item =>
                    context.CallActivityAsync<CatalogueItem>(
                        SaleFinderFunctionNames.FillSaleFinderItemPrice,
                        new FillSaleFinderItemPriceInput(catalogueDownloadInfo.CurrencyCulture, item)
                    )
                )
            ).ConfigureAwait(true);

            var catalogueWithPrices = downloadedCatalogue with { Items = itemsWithPrices };
            #endregion

            #region Filter catalouge items and send digest email
            context.SetCustomStatus("FilteringAndSendingDigestEmail");

            await context.CallSubOrchestratorAsync(
                CoreFunctionNames.FilterCatalogueAndSendDigestEmail,
                new FilterCatalogueAndSendDigestEmailInput(catalogueWithPrices, scanStateId)
            ).ConfigureAwait(true);
            #endregion

            #region Update catalogue's scan state
            context.SetCustomStatus("UpdatingState");
            log.UpdatingState(scanStateId.Key);

            await context.Entities.UpdateScanStateAsync(scanStateId, ScanState.Completed).ConfigureAwait(true);
            #endregion

            log.Completed(scanStateId.Key);
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
    /// Function that triggers the <see cref="RunOrchestrator"/> orchestrator function when a SaleFinder catalogue is queued for scanning.
    /// </summary>
    [Function(SaleFinderFunctionNames.ScanSaleFinderCatalogue_QueueStart)]
    public static async Task QueueStart(
        [QueueTrigger(SaleFinderQueueNames.SaleFinderCataloguesToScan)] SaleFinderCatalogueDownloadInformation downloadInformation,
        [DurableClient] DurableTaskClient durableTaskClient,
        FunctionContext context,
        CancellationToken cancellationToken
    )
    {
        #region null checks
        ArgumentNullException.ThrowIfNull(downloadInformation);

        ArgumentNullException.ThrowIfNull(durableTaskClient);

        ArgumentNullException.ThrowIfNull(context);
        #endregion

        var logger = context.GetLogger(typeof(ScanSaleFinderCatalogue).FullName!);

        var instanceId = await durableTaskClient.ScheduleNewOrchestrationInstanceAsync(
            SaleFinderFunctionNames.ScanSaleFinderCatalogue,
            downloadInformation,
            cancellationToken
        ).ConfigureAwait(false);

        logger.StartedOrchestration(instanceId);
    }
}