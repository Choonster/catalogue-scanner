using CatalogueScanner.Dto.StorageEntity;
using CatalogueScanner.SharedCode.Dto.StorageEntity;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace CatalogueScanner
{
    /// <summary>
    /// Downloads a SaleFinder catalogue and outputs the items.
    /// </summary>
    public class DownloadSaleFinderCatalogue
    {
        private readonly SaleFinderService saleFinderService;

        public DownloadSaleFinderCatalogue(SaleFinderService saleFinderService)
        {
            this.saleFinderService = saleFinderService;
        }

        [FunctionName("DownloadSalesFinderData")]
        public async Task RunAsync(
            [QueueTrigger(Constants.QueueNames.SaleFinderCataloguesToDownload)]
            SaleFinderCatalogueDownloadInformation downloadInformation,
            ILogger log,
            [Queue(Constants.QueueNames.DownloadedItems)]
            ICollector<CatalogueItem> collector
        )
        {
            #region null checks
            if (downloadInformation is null)
            {
                throw new ArgumentNullException(nameof(downloadInformation));
            }

            if (log is null)
            {
                throw new ArgumentNullException(nameof(log));
            }

            if (collector is null)
            {
                throw new ArgumentNullException(nameof(collector));
            }
            #endregion

            var catalogue = await saleFinderService.GetCatalogueAsync(downloadInformation.SaleId).ConfigureAwait(false);

            log.LogInformation($"Successfully downloaded and parsed catalogue with {catalogue.Pages.Count} pages");

            var items = catalogue.Pages
                .SelectMany(page => page.Items)
                .Where(item => item.ItemId.HasValue);

            Parallel.ForEach(
                items,
                item => collector.Add(new CatalogueItem
                {
                    Id = item.ItemId.Value.ToString(CultureInfo.InvariantCulture),
                    Name = item.ItemName,
                    Sku = item.Sku,
                    Uri = new Uri(downloadInformation.BaseUri, item.ItemUrl),
                })
            );
        }
    }
}
