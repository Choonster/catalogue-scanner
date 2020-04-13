using CatalogueScanner.Dto.StorageEntity;
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

        [FunctionName(Constants.FunctionNames.DownloadSaleFinderCatalogue)]
        public async Task<Catalogue> RunAsync(
            [ActivityTrigger] SaleFinderCatalogueDownloadInformation downloadInformation,
            ILogger log
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
            #endregion

            var catalogue = await saleFinderService.GetCatalogueAsync(downloadInformation.SaleId).ConfigureAwait(false);

            log.LogInformation($"Successfully downloaded and parsed catalogue with {catalogue.Pages.Count} pages");

            var items = catalogue.Pages
                .SelectMany(page => page.Items)
                .Where(item => item.ItemId.HasValue)
                .Select(item => new CatalogueItem
                {
                    Id = item.ItemId!.Value.ToString(CultureInfo.InvariantCulture),
                    Name = item.ItemName,
                    Sku = item.Sku,
                    Uri = item.ItemUrl != null ? new Uri(downloadInformation.BaseUri, item.ItemUrl) : null,
                })
                .ToList();

            return new Catalogue(downloadInformation.Store, catalogue.StartDate, catalogue.EndDate, items);
        }
    }
}
