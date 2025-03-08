using CatalogueScanner.Core;
using CatalogueScanner.SaleFinder.Dto.FunctionResult;
using CatalogueScanner.SaleFinder.Dto.SaleFinder;
using CatalogueScanner.SaleFinder.Service;
using HtmlAgilityPack;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace CatalogueScanner.SaleFinder.Functions;

/// <summary>
/// Checks for new Big W catalogues and queues them for scanning.
/// </summary>
public class CheckBigWCatalogue(SaleFinderService saleFinderService, ILogger<CheckBigWCatalogue> logger)
{
    private const int BigWStoreId = 128;
    private const int BigWLocationId = -1; // Big W doesn't seem to use location IDs
    private const string BigWStoreName = "Big W";
    private static readonly Uri CatalaogueBaseUri = new("https://www.bigw.com.au/bigw-catalogues");

    private readonly SaleFinderService saleFinderService = saleFinderService ?? throw new ArgumentNullException(nameof(saleFinderService));
    private readonly ILogger<CheckBigWCatalogue> logger = logger ?? throw new ArgumentNullException(nameof(logger));

    [Function(SaleFinderFunctionNames.CheckBigWCatalogue)]
    [QueueOutput(SaleFinderQueueNames.SaleFinderCataloguesToScan)]
    public async Task<SaleFinderCatalogueDownloadInformation[]> RunAsync(
        [TimerTrigger("%" + SaleFinderAppSettingNames.CheckCatalogueFunctionCronExpression + "%")] TimerInfo timer,
        CancellationToken cancellationToken
    )
    {
        #region null checks
        ArgumentNullException.ThrowIfNull(timer);
        #endregion

        var viewResponse = await saleFinderService.GetCatalogueViewDataAsync(BigWStoreId, BigWLocationId, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("viewResponse is null");

        var saleIds = FindSaleIds(viewResponse).ToList();

        logger.FoundSaleIds(saleIds);

        return [.. saleIds.Select(saleId => new SaleFinderCatalogueDownloadInformation(saleId, CatalaogueBaseUri, BigWStoreName, CurrencyCultures.AustralianDollar))];
    }

    private static IEnumerable<int> FindSaleIds(CatalogueViewResponse viewResponse)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(viewResponse.Content);

        var viewLinks = doc.DocumentNode
            .Descendants("a")
            .Where(node => node.Attributes["href"] != null)
            .Where(node => node.HasClass("readbutton"))
            .ToList();

        if (viewLinks.Count == 0)
        {
            throw new UnableToFindSaleIdException($"Didn't find .readbutton links in HTML content.\n\n{viewResponse.Content}");
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
