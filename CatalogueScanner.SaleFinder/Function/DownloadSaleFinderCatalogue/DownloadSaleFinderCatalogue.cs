using CatalogueScanner.Core.Dto.FunctionResult;
using CatalogueScanner.SaleFinder.DTO.FunctionResult;
using CatalogueScanner.SaleFinder.Service;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace CatalogueScanner.SaleFinder.Function
{
    /// <summary>
    /// Downloads a SaleFinder catalogue and outputs the items.
    /// </summary>
    public class DownloadSaleFinderCatalogue
    {
        private readonly SaleFinderService saleFinderService;
        private readonly IStringLocalizer<DownloadSaleFinderCatalogue> S;

        public DownloadSaleFinderCatalogue(SaleFinderService saleFinderService, IStringLocalizer<DownloadSaleFinderCatalogue> stringLocalizer)
        {
            this.saleFinderService = saleFinderService;
            S = stringLocalizer;
        }

        [FunctionName(SaleFinderFunctionNames.DownloadSaleFinderCatalogue)]
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

            log.LogInformation(S.Plural(catalogue.Pages.Count, "Successfully downloaded and parsed catalogue with 1 page", "Successfully downloaded and parsed catalogue with {0} pages", catalogue.Pages.Count));

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
