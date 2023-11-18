using CatalogueScanner.Core.Dto.FunctionResult;
using CatalogueScanner.Core.Localisation;
using CatalogueScanner.SaleFinder.Dto.FunctionResult;
using CatalogueScanner.SaleFinder.Service;
using Microsoft.Azure.Functions.Worker;
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
    public class DownloadSaleFinderCatalogue(
        SaleFinderService saleFinderService,
        ILogger<DownloadSaleFinderCatalogue> logger,
        IPluralStringLocalizer<DownloadSaleFinderCatalogue> pluralStringLocalizer
        )
    {
        private readonly SaleFinderService saleFinderService = saleFinderService ?? throw new ArgumentNullException(nameof(saleFinderService));
        private readonly ILogger<DownloadSaleFinderCatalogue> logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IPluralStringLocalizer<DownloadSaleFinderCatalogue> S = pluralStringLocalizer ?? throw new ArgumentNullException(nameof(pluralStringLocalizer));

        [Function(SaleFinderFunctionNames.DownloadSaleFinderCatalogue)]
        public async Task<Catalogue> RunAsync(
            [ActivityTrigger] SaleFinderCatalogueDownloadInformation downloadInformation,
            CancellationToken cancellationToken
        )
        {
            #region null checks
            ArgumentNullException.ThrowIfNull(downloadInformation);
            #endregion

            var catalogue = await saleFinderService.GetCatalogueAsync(downloadInformation.SaleId, cancellationToken).ConfigureAwait(false)
                ?? throw new InvalidOperationException("catalogue is null");

            logger.LogInformation(S.Plural(catalogue.Pages.Count, "Successfully downloaded and parsed catalogue with 1 page", "Successfully downloaded and parsed catalogue with {0} pages", catalogue.Pages.Count));

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
