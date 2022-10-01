using CatalogueScanner.Core;
using CatalogueScanner.SaleFinder.Dto.FunctionResult;
using CatalogueScanner.SaleFinder.Dto.SaleFinder;
using CatalogueScanner.SaleFinder.Options;
using CatalogueScanner.SaleFinder.Service;
using HtmlAgilityPack;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CatalogueScanner.SaleFinder.Functions
{
    public class CheckWoolworthsCatalogue
    {
        private const int WoolworthsStoreId = 126;
        private const string WoolworthsStoreName = "Woolworths";
        private static readonly Uri CatalaogueBaseUri = new("https://www.woolworths.com.au/shop/catalogue/view");

        private readonly SaleFinderService saleFinderService;
        private readonly WoolworthsOptions options;
        private readonly IStringLocalizer<CheckWoolworthsCatalogue> S;

        public CheckWoolworthsCatalogue(SaleFinderService saleFinderService, IOptionsSnapshot<WoolworthsOptions> optionsAccessor, IStringLocalizer<CheckWoolworthsCatalogue> stringLocalizer)
        {
            #region null checks
            if (optionsAccessor is null)
            {
                throw new ArgumentNullException(nameof(optionsAccessor));
            }
            #endregion

            this.saleFinderService = saleFinderService ?? throw new ArgumentNullException(nameof(saleFinderService));
            options = optionsAccessor.Value;
            S = stringLocalizer ?? throw new ArgumentNullException(nameof(stringLocalizer));
        }

        [FunctionName(SaleFinderFunctionNames.CheckWoolworthsCatalogue)]
        public async Task RunAsync(
            [TimerTrigger("%" + SaleFinderAppSettingNames.CheckCatalogueFunctionCronExpression + "%")] TimerInfo timer,
            ILogger log,
            [Queue(SaleFinderQueueNames.SaleFinderCataloguesToScan)] IAsyncCollector<SaleFinderCatalogueDownloadInformation> collector,
            CancellationToken cancellationToken
        )
        {
            #region null checks
            if (timer is null)
            {
                throw new ArgumentNullException(nameof(timer));
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

            var viewResponse = await saleFinderService.GetCatalogueViewDataAsync(WoolworthsStoreId, options.SaleFinderLocationId, cancellationToken).ConfigureAwait(false);

            if (viewResponse is null)
            {
                throw new InvalidOperationException("viewResponse is null");
            }

            var saleIds = FindSaleIds(viewResponse).ToList();

            log.LogInformation(S["Found sale IDs: {0}"], saleIds);

            foreach (var saleId in saleIds)
            {
                await collector.AddAsync(new SaleFinderCatalogueDownloadInformation(saleId, CatalaogueBaseUri, WoolworthsStoreName, CurrencyCultures.AustralianDollar), cancellationToken).ConfigureAwait(false);
            }
        }

        private IEnumerable<int> FindSaleIds(CatalogueViewResponse viewResponse)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(viewResponse.Content);

            var catalogueContainers = doc.DocumentNode
                .Descendants("div")
                .Where(node => node.HasClass("sale-container"))
                .Where(node => node.GetDataAttribute("saleid") != null)
                .ToList();

            if (!catalogueContainers.Any())
            {
                throw new UnableToFindSaleIdException($"{S["Didn't find .sale-container elements in HTML content."]}\n\n{viewResponse.Content}");
            }

            foreach (var catalogueContainer in catalogueContainers)
            {
                var saleIdValue = catalogueContainer.GetDataAttribute("saleid").Value;

                if (!int.TryParse(saleIdValue, out var saleId))
                {
                    throw new UnableToFindSaleIdException($"{S["Found invalid \"{0}\" attribute value \"{1}\" in HTML content.", "data-saleid", saleIdValue]}\n\n{viewResponse.Content}");
                }

                yield return saleId;
            }
        }
    }
}
