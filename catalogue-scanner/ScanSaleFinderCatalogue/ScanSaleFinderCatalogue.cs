using CatalogueScanner.Dto.FunctionResult;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CatalogueScanner
{
    public static class ScanSaleFinderCatalogue
    {
        /// <summary>
        /// Orchestrator function that downloads a SaleFinder catalogue, filters the items using the configured rules and sends an email digest.
        /// </summary>
        [FunctionName(Constants.FunctionNames.ScanSaleFinderCatalogue)]
        public static async Task RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            #region null checks
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            #endregion

            var catalogueDownloadInfo = context.GetInput<SaleFinderCatalogueDownloadInformation>();

            // TODO: Check for duplicates

            var downloadedCatalogue = await context.CallActivityAsync<Catalogue>(Constants.FunctionNames.DownloadSaleFinderCatalogue, catalogueDownloadInfo).ConfigureAwait(true);

            var itemTasks = downloadedCatalogue.Items
                .Select(item => context.CallActivityAsync<CatalogueItem?>(Constants.FunctionNames.FilterCatalogueItem, item))
                .ToList();

            await Task.WhenAll(itemTasks).ConfigureAwait(true);

            var filteredItems = itemTasks
                .Where(task => task.Result != null)
                .Select(task => task.Result!)
                .ToList();

            var filteredCatalogue = new Catalogue(downloadedCatalogue.Store, downloadedCatalogue.StartDate, downloadedCatalogue.EndDate, filteredItems);

            await context.CallActivityAsync(Constants.FunctionNames.SendCatalogueDigestEmail, filteredCatalogue).ConfigureAwait(true);
        }

        /// <summary>
        /// Function that triggers the <see cref="RunOrchestrator"/> orchestrator function when a SaleFinder catalogue is queued for scanning.
        /// </summary>
        [FunctionName(Constants.FunctionNames.ScanSaleFinderCatalogue_QueueStart)]
        public static async Task QueueStart(
            [QueueTrigger(Constants.QueueNames.SaleFinderCataloguesToScan)] SaleFinderCatalogueDownloadInformation downloadInformation,
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

            string instanceId = await starter.StartNewAsync(Constants.FunctionNames.ScanSaleFinderCatalogue, downloadInformation).ConfigureAwait(false);

            log.LogInformation($"Started {Constants.FunctionNames.ScanSaleFinderCatalogue} orchestration with ID = '{instanceId}'.");
        }
    }
}