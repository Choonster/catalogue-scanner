using CatalogueScanner.Core.Dto.FunctionResult;
using CatalogueScanner.Core.Localisation;
using CatalogueScanner.SaleFinder.Dto.FunctionResult;
using CatalogueScanner.SaleFinder.Service;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CatalogueScanner.SaleFinder.Functions
{
    /// <summary>
    /// Downloads a SaleFinder catalogue and outputs the items.
    /// </summary>
    public class DownloadSaleFinderCatalogue
    {
        private readonly SaleFinderService saleFinderService;
        private readonly IPluralStringLocalizer<DownloadSaleFinderCatalogue> S;

        public DownloadSaleFinderCatalogue(SaleFinderService saleFinderService, IPluralStringLocalizer<DownloadSaleFinderCatalogue> pluralStringLocalizer)
        {
            this.saleFinderService = saleFinderService;
            S = pluralStringLocalizer;
        }

        [FunctionName(SaleFinderFunctionNames.DownloadSaleFinderCatalogue)]
        public async Task<Catalogue> RunAsync(
            [ActivityTrigger] SaleFinderCatalogueDownloadInformation downloadInformation,
            ILogger log,
            CancellationToken cancellationToken
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

            var catalogue = await saleFinderService.GetCatalogueAsync(downloadInformation.SaleId, cancellationToken).ConfigureAwait(false);

            if (catalogue is null)
            {
                throw new InvalidOperationException("catalogue is null");
            }

            log.LogInformation(S.Plural(catalogue.Pages.Count, "Successfully downloaded and parsed catalogue with 1 page", "Successfully downloaded and parsed catalogue with {0} pages", catalogue.Pages.Count));

            var items = catalogue.Pages
                .SelectMany(page => page.Items)
                .Where(item => item.ItemId.HasValue)
                .Select(item =>
                    new CatalogueItem(
                        item.ItemId!.Value.ToString(CultureInfo.InvariantCulture),
                        item.ItemName,
                        item.Sku,
                        item.ItemUrl != null ? new Uri(downloadInformation.BaseUri, item.ItemUrl) : null,
                        null,
                        null,
                        null
                    )
                )
                .ToList();

            return new Catalogue(downloadInformation.Store, catalogue.StartDate, catalogue.EndDate, downloadInformation.CurrencyCulture, items);
        }
    }
}
