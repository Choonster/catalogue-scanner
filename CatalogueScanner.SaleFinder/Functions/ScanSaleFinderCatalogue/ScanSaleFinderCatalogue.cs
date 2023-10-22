using CatalogueScanner.Core;
using CatalogueScanner.Core.Dto.EntityKey;
using CatalogueScanner.Core.Dto.FunctionInput;
using CatalogueScanner.Core.Dto.FunctionResult;
using CatalogueScanner.Core.Functions.Entity;
using CatalogueScanner.SaleFinder.Dto.FunctionInput;
using CatalogueScanner.SaleFinder.Dto.FunctionResult;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace CatalogueScanner.SaleFinder.Functions
{
    public static class ScanSaleFinderCatalogue
    {
        private const string CatalogueType = "SaleFinder";

        /// <summary>
        /// Orchestrator function that downloads a SaleFinder catalogue, filters the items using the configured rules and sends an email digest.
        /// </summary>
        [FunctionName(SaleFinderFunctionNames.ScanSaleFinderCatalogue)]
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

            var catalogueDownloadInfo = context.GetInput<SaleFinderCatalogueDownloadInformation>();

            var scanStateId = ICatalogueScanState.CreateId(new CatalogueScanStateKey(CatalogueType, catalogueDownloadInfo.Store, catalogueDownloadInfo.SaleId.ToString(CultureInfo.InvariantCulture)));
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

                var downloadedCatalogue = await context.CallActivityAsync<Catalogue>(SaleFinderFunctionNames.DownloadSaleFinderCatalogue, catalogueDownloadInfo).ConfigureAwait(true);
                #endregion

                #region Fill prices
                context.SetCustomStatus("FillingPrices");
                log.LogDebug($"Filling Prices - {scanStateId.EntityKey}");

                var itemsWithPrices = await Task.WhenAll(
                    downloadedCatalogue.Items.Select(item =>
                        context.CallActivityAsync<CatalogueItem>(
                            SaleFinderFunctionNames.FillSaleFinderItemPrice,
                            new FillSaleFinderItemPriceInput(catalogueDownloadInfo.CurrencyCulture, item)
                        )
                    )
                ).ConfigureAwait(true);
                #endregion

                #region Filter catalouge items and send digest email
                context.SetCustomStatus("FilteringAndSendingDigestEmail");

                var catalogueWithPrices = downloadedCatalogue with { Items =  itemsWithPrices };

                await context.CallSubOrchestratorAsync(
                    CoreFunctionNames.FilterCatalogueAndSendDigestEmail,
                    new FilterCatalogueAndSendDigestEmailInput(catalogueWithPrices, scanStateId)
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
        /// Function that triggers the <see cref="RunOrchestrator"/> orchestrator function when a SaleFinder catalogue is queued for scanning.
        /// </summary>
        [FunctionName(SaleFinderFunctionNames.ScanSaleFinderCatalogue_QueueStart)]
        public static async Task QueueStart(
            [QueueTrigger(SaleFinderQueueNames.SaleFinderCataloguesToScan)] SaleFinderCatalogueDownloadInformation downloadInformation,
            [DurableClient] IDurableClient starter,
            ILogger log
        )
        {
            #region null checks
            if (downloadInformation is null)
            {
                throw new ArgumentNullException(nameof(downloadInformation));
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

            string instanceId = await starter.StartNewAsync(SaleFinderFunctionNames.ScanSaleFinderCatalogue, downloadInformation).ConfigureAwait(false);

            log.LogInformation($"Started {SaleFinderFunctionNames.ScanSaleFinderCatalogue} orchestration with ID = '{instanceId}'.");
        }
    }
}