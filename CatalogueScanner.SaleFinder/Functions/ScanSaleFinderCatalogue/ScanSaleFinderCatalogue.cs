using CatalogueScanner.Core;
using CatalogueScanner.Core.Dto.FunctionResult;
using CatalogueScanner.Core.Entity;
using CatalogueScanner.Core.Entity.Implementation;
using CatalogueScanner.SaleFinder.DTO.FunctionResult;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CatalogueScanner.SaleFinder.Functions.ScanSaleFinderCatalogue
{
    public static class ScanSaleFinderCatalogue
    {
        private const string CatalogueType = "SaleFinder";

        /// <summary>
        /// Orchestrator function that downloads a SaleFinder catalogue, filters the items using the configured rules and sends an email digest.
        /// </summary>
        [FunctionName(SaleFinderConstants.FunctionNames.ScanSaleFinderCatalogue)]
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

            var scanStateId = new EntityId(nameof(CatalogueScanState), $"{CatalogueType}|{catalogueDownloadInfo.Store}|{catalogueDownloadInfo.SaleId}");
            var scanState = context.CreateEntityProxy<ICatalogueScanState>(scanStateId);

            using (await context.LockAsync(scanStateId).ConfigureAwait(true))
            {
                #region Check and update the catalogue's scan state
                context.SetCustomStatus("CheckingState");

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

                var downloadedCatalogue = await context.CallActivityAsync<Catalogue>(SaleFinderConstants.FunctionNames.DownloadSaleFinderCatalogue, catalogueDownloadInfo).ConfigureAwait(true);
                #endregion

                #region Filter catalouge items
                context.SetCustomStatus("Filtering");

                var itemTasks = downloadedCatalogue.Items
                    .Select(item => context.CallActivityAsync<CatalogueItem?>(CoreConstants.FunctionNames.FilterCatalogueItem, item))
                    .ToList();

                await Task.WhenAll(itemTasks).ConfigureAwait(true);
                #endregion

                #region Send digest email
                context.SetCustomStatus("SendingDigestEmail");

                var filteredItems = itemTasks
                    .Where(task => task.Result != null)
                    .Select(task => task.Result!)
                    .ToList();

                if (filteredItems.Any())
                {
                    var filteredCatalogue = new Catalogue(downloadedCatalogue.Store, downloadedCatalogue.StartDate, downloadedCatalogue.EndDate, filteredItems);

                    await context.CallActivityAsync(CoreConstants.FunctionNames.SendCatalogueDigestEmail, filteredCatalogue).ConfigureAwait(true);
                }
                else
                {
                    log.LogInformation($"Catalogue {scanStateId.EntityKey} had no matching items, skipping digest email.");
                }
                #endregion

                #region Update catalogue's scan state
                context.SetCustomStatus("UpdatingState");

                await scanState.UpdateState(ScanState.Completed).ConfigureAwait(true);
                #endregion

                context.SetCustomStatus("Completed");
            }
        }

        /// <summary>
        /// Function that triggers the <see cref="RunOrchestrator"/> orchestrator function when a SaleFinder catalogue is queued for scanning.
        /// </summary>
        [FunctionName(SaleFinderConstants.FunctionNames.ScanSaleFinderCatalogue_QueueStart)]
        public static async Task QueueStart(
            [QueueTrigger(SaleFinderConstants.QueueNames.SaleFinderCataloguesToScan)] SaleFinderCatalogueDownloadInformation downloadInformation,
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

            string instanceId = await starter.StartNewAsync(SaleFinderConstants.FunctionNames.ScanSaleFinderCatalogue, downloadInformation).ConfigureAwait(false);

            log.LogInformation($"Started {SaleFinderConstants.FunctionNames.ScanSaleFinderCatalogue} orchestration with ID = '{instanceId}'.");
        }
    }
}