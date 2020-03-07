using CatalogueScanner.Dto.SaleFinder;
using CatalogueScanner.SharedCode.Dto.StorageEntity;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CatalogueScanner.DownloadSaleFinderCatalogue
{
    public class DownloadSaleFinderCatalogue
    {
        private readonly SaleFinderService saleFinderService;

        public DownloadSaleFinderCatalogue(SaleFinderService saleFinderService)
        {
            this.saleFinderService = saleFinderService;
        }

        [FunctionName("DownloadSalesFinderData")]
        public async Task RunAsync(
            [QueueTrigger(Constants.QueueNames.CataloguesToDownload)]
            SaleFinderCatalogueDownloadInformation downloadInformation,
            ILogger log,
            [Queue(Constants.QueueNames.DownloadedItems)]
            ICollector<Item> collector
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

            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            var result = await saleFinderService.GetCatalogueAsync(downloadInformation.SaleId).ConfigureAwait(false);

            log.LogInformation($"Successfully downloaded and parsed catalogue with {result.Pages.Count} pages");

            Parallel.ForEach(
                result.Pages.SelectMany(page => page.Items),
                item => collector.Add(item)
            );
        }
    }
}
