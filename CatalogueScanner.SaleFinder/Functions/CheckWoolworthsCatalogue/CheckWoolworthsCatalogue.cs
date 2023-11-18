using CatalogueScanner.Core;
using CatalogueScanner.SaleFinder.Dto.FunctionResult;
using CatalogueScanner.SaleFinder.Dto.SaleFinder;
using CatalogueScanner.SaleFinder.Options;
using CatalogueScanner.SaleFinder.Service;
using HtmlAgilityPack;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CatalogueScanner.SaleFinder.Functions;

public class CheckWoolworthsCatalogue
{
    private const int WoolworthsStoreId = 126;
    private const string WoolworthsStoreName = "Woolworths";
    private static readonly Uri CatalaogueBaseUri = new("https://www.woolworths.com.au/shop/catalogue/view");

    private readonly SaleFinderService saleFinderService;
    private readonly WoolworthsOptions options;
    private readonly ILogger<CheckWoolworthsCatalogue> logger;

    public CheckWoolworthsCatalogue(SaleFinderService saleFinderService, IOptionsSnapshot<WoolworthsOptions> optionsAccessor, ILogger<CheckWoolworthsCatalogue> logger)
    {
        #region null checks
        ArgumentNullException.ThrowIfNull(optionsAccessor);
        #endregion

        this.saleFinderService = saleFinderService ?? throw new ArgumentNullException(nameof(saleFinderService));
        options = optionsAccessor.Value;
        this.logger = logger;
    }

    [Function(SaleFinderFunctionNames.CheckWoolworthsCatalogue)]
    [QueueOutput(SaleFinderQueueNames.SaleFinderCataloguesToScan)]
    public async Task<SaleFinderCatalogueDownloadInformation[]> RunAsync(
        [TimerTrigger("%" + SaleFinderAppSettingNames.CheckCatalogueFunctionCronExpression + "%")] TimerInfo timer,
        CancellationToken cancellationToken
    )
    {
        #region null checks
        ArgumentNullException.ThrowIfNull(timer);
        #endregion

        var viewResponse = await saleFinderService.GetCatalogueViewDataAsync(WoolworthsStoreId, options.SaleFinderLocationId, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("viewResponse is null");

        var saleIds = FindSaleIds(viewResponse).ToList();

        logger.FoundSaleIds(saleIds);

        return saleIds
            .Select(saleId => new SaleFinderCatalogueDownloadInformation(saleId, CatalaogueBaseUri, WoolworthsStoreName, CurrencyCultures.AustralianDollar))
            .ToArray();
    }

    private static IEnumerable<int> FindSaleIds(CatalogueViewResponse viewResponse)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(viewResponse.Content);

        var catalogueContainers = doc.DocumentNode
            .Descendants("div")
            .Where(node => node.HasClass("sale-container"))
            .Where(node => node.GetDataAttribute("saleid") != null)
            .ToList();

        if (catalogueContainers.Count == 0)
        {
            throw new UnableToFindSaleIdException($"Didn't find .sale-container elements in HTML content.\n\n{viewResponse.Content}");
        }

        foreach (var catalogueContainer in catalogueContainers)
        {
            var saleIdValue = catalogueContainer.GetDataAttribute("saleid").Value;

            if (!int.TryParse(saleIdValue, out var saleId))
            {
                throw new UnableToFindSaleIdException($"Found invalid \"data-saleid\" attribute value \"{saleIdValue}\" in HTML content.\n\n{viewResponse.Content}");
            }

            yield return saleId;
        }
    }
}
