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
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CatalogueScanner.SaleFinder.Functions;

/// <summary>
/// Checks for new Coles catalogues and queues them for scanning.
/// </summary>
public class CheckColesCatalogue
{
    private const int ColesStoreId = 148;
    private const string ColesStoreName = "Coles";
    private static readonly Uri CatalaogueBaseUri = new("https://www.coles.com.au/catalogues-and-specials/view-all-available-catalogues");

    private readonly SaleFinderService saleFinderService;
    private readonly ColesOptions options;
    private readonly ILogger<CheckColesCatalogue> logger;

    public CheckColesCatalogue(SaleFinderService saleFinderService, IOptionsSnapshot<ColesOptions> optionsAccessor, ILogger<CheckColesCatalogue> logger)
    {
        #region null checks
        ArgumentNullException.ThrowIfNull(optionsAccessor);
        #endregion

        this.saleFinderService = saleFinderService ?? throw new ArgumentNullException(nameof(saleFinderService));
        options = optionsAccessor.Value;
        this.logger = logger;
    }

    [Function(SaleFinderFunctionNames.CheckColesCatalogue)]
    [QueueOutput(SaleFinderQueueNames.SaleFinderCataloguesToScan)]
    public async Task<SaleFinderCatalogueDownloadInformation[]> RunAsync(
        [TimerTrigger("%" + SaleFinderAppSettingNames.CheckCatalogueFunctionCronExpression + "%")] TimerInfo timer,
        CancellationToken cancellationToken
    )
    {
        #region null checks
        ArgumentNullException.ThrowIfNull(timer);
        #endregion

        var viewResponse = await saleFinderService.GetCatalogueViewDataAsync(ColesStoreId, options.SaleFinderLocationId, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("viewResponse is null");

        var saleIds = FindSaleIds(viewResponse).ToList();

        logger.FoundSaleIds(saleIds);

        return saleIds
            .Select(saleId => new SaleFinderCatalogueDownloadInformation(saleId, CatalaogueBaseUri, ColesStoreName, CurrencyCultures.AustralianDollar))
            .ToArray();
    }

    private static IEnumerable<int> FindSaleIds(CatalogueViewResponse viewResponse)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(viewResponse.Content);

        var viewLinks = doc.DocumentNode
            .Descendants("a")
            .Where(node => node.Attributes["href"] != null)
            .Where(node => node.HasClass("sf-view-button"))
            .ToList();

        if (viewLinks.Count == 0)
        {
            throw new UnableToFindSaleIdException($"Didn't find .sf-view-button links in HTML content.\n\n{viewResponse.Content}");
        }

        foreach (var viewLink in viewLinks)
        {
            var url = new Uri(CatalaogueBaseUri, viewLink.Attributes["href"].Value);

            foreach (var param in url.Fragment.Split('&'))
            {
                var parts = param.Split('=');

                if (parts.Length < 2)
                {
                    continue;
                }

                var name = parts[0];
                var value = parts[1];

                if (name == "saleId")
                {
                    yield return int.Parse(value, CultureInfo.InvariantCulture);
                    break;
                }
            }
        }
    }
}
