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
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace CatalogueScanner.SaleFinder.Functions
{
    /// <summary>
    /// Checks for new Coles catalogues and queues them for scanning.
    /// </summary>
    public class CheckColesCatalogue
    {
        private const int ColesStoreId = 148;
        private const string ColesStoreName = "Coles";
        private const string ThisWeeksCatalogue = "This week's catalogue";
        private static readonly Uri CatalaogueBaseUri = new("https://www.coles.com.au/catalogues-and-specials/view-all-available-catalogues");

        private readonly SaleFinderService saleFinderService;
        private readonly ColesOptions options;
        private readonly IStringLocalizer<CheckColesCatalogue> S;

        public CheckColesCatalogue(SaleFinderService saleFinderService, IOptionsSnapshot<ColesOptions> optionsAccessor, IStringLocalizer<CheckColesCatalogue> stringLocalizer)
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

        [FunctionName(SaleFinderFunctionNames.CheckColesCatalogue)]
        [return: Queue(SaleFinderQueueNames.SaleFinderCataloguesToScan)]
        public async Task<SaleFinderCatalogueDownloadInformation> RunAsync(
            [TimerTrigger("%" + SaleFinderAppSettingNames.CheckCatalogueFunctionCronExpression + "%")] TimerInfo timer,
            ILogger log
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
            #endregion

            var viewResponse = await saleFinderService.GetCatalogueViewDataAsync(ColesStoreId, options.SaleFinderLocationId).ConfigureAwait(false);

            var saleId = FindSaleId(viewResponse);

            log.LogInformation(S["Found sale ID: {0}", saleId]);

            return new SaleFinderCatalogueDownloadInformation(saleId, CatalaogueBaseUri, ColesStoreName);
        }

        private int FindSaleId(CatalogueViewResponse viewResponse)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(viewResponse.Content);

            var thisWeeksCatalogueHeader = doc.DocumentNode
                .Descendants("h3")
                .Where(node => node.HasClass("sale-name-cell"))
                .Where(node => HtmlEntity.DeEntitize(node.InnerText) == ThisWeeksCatalogue)
                .FirstOrDefault();

            if (thisWeeksCatalogueHeader is null)
            {
                throw new UnableToFindSaleIdException($"{S["Didn't find \"{0}\" cell in HTML content.", ThisWeeksCatalogue]}\n\n{viewResponse.Content}");
            }

            var parentTile = thisWeeksCatalogueHeader
                .Ancestors("div")
                .Where(node => node.HasClass("sf-catalogues-tile"))
                .FirstOrDefault();

            if (parentTile is null)
            {
                throw new UnableToFindSaleIdException($"{S["Didn't find \"{0}\" parent tile in HTML content.", ThisWeeksCatalogue]}\n\n{viewResponse.Content}");
            }

            var viewLink = parentTile
                .Descendants("a")
                .Where(node => node.Attributes["href"] != null)
                .Where(node => node.HasClass("sf-view-button"))
                .FirstOrDefault();

            if (viewLink is null)
            {
                throw new UnableToFindSaleIdException($"{S["Didn't find \"{0}\" link in HTML content.", "View"]}\n\n{viewResponse.Content}");
            }

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
                    return int.Parse(value, CultureInfo.InvariantCulture);
                }
            }

            throw new UnableToFindSaleIdException($"{S["Didn't find \"{0}\" parameter in \"{1}\" link in HTML content.", "saleId", "View"]}\n\n{viewResponse.Content}");
        }
    }
}
