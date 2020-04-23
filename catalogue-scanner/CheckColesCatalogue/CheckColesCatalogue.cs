using CatalogueScanner.Dto.Config;
using CatalogueScanner.Dto.FunctionResult;
using CatalogueScanner.Dto.SaleFinder;
using HtmlAgilityPack;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace CatalogueScanner
{
    /// <summary>
    /// Checks for new Coles catalogues and queues them for scanning.
    /// </summary>
    public class CheckColesCatalogue
    {
        private const int ColesStoreId = 148;
        private const string ColesStoreName = "Coles";
        private static readonly Uri CatalaogueBaseUri = new Uri("https://www.coles.com.au/catalogues-and-specials/view-all-available-catalogues");

        private readonly SaleFinderService saleFinderService;
        private readonly ColesSettings settings;
        private readonly IStringLocalizer<CheckColesCatalogue> S;

        public CheckColesCatalogue(SaleFinderService saleFinderService, IOptionsSnapshot<CatalogueScannerSettings> settings, IStringLocalizer<CheckColesCatalogue> stringLocalizer)
        {
            #region null checks
            if (settings is null)
            {
                throw new ArgumentNullException(nameof(settings));
            }
            #endregion

            this.saleFinderService = saleFinderService ?? throw new ArgumentNullException(nameof(saleFinderService));
            this.settings = settings.Value.Coles;
            S = stringLocalizer ?? throw new ArgumentNullException(nameof(stringLocalizer));
        }

        [FunctionName(Constants.FunctionNames.CheckColesCatalogue)]
        [return: Queue(Constants.QueueNames.SaleFinderCataloguesToScan)]
        public async Task<SaleFinderCatalogueDownloadInformation> RunAsync(
            [TimerTrigger("0 */5 * * * *")] TimerInfo timer,
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

            var viewResponse = await saleFinderService.GetCatalogueViewDataAsync(ColesStoreId, settings.SaleFinderLocationId).ConfigureAwait(false);

            var saleId = FindSaleId(viewResponse);

            log.LogInformation(S["Found sale ID: {0}", saleId]);

            return new SaleFinderCatalogueDownloadInformation(saleId, CatalaogueBaseUri, ColesStoreName);
        }

        private int FindSaleId(CatalogueViewResponse viewResponse)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(viewResponse.Content);

            var thisWeeksCatalogueHeader = doc.DocumentNode
                .Descendants("div")
                .Where(node => node.HasClass("sale-name-cell"))
                .Where(node => node.InnerText == "This week&#39;s catalogue")
                .FirstOrDefault();

            if (thisWeeksCatalogueHeader is null)
            {
                throw new UnableToFindSaleIdException($"{S["Didn't find \"{0}\" cell in HTML content.", "This week's catalogue"]}\n\n{viewResponse.Content}");
            }

            var viewLink = thisWeeksCatalogueHeader.ParentNode
                .Descendants("a")
                .Where(node => node.Attributes["href"] != null)
                .Where(node => node.HasClass("sf-view-button"))
                .Where(node => node.HasClass("button-secondary"))
                .FirstOrDefault();

            if (viewLink is null)
            {
                throw new UnableToFindSaleIdException($"{S["Didn't find \"{0}\" link in HTML content.", "View"]}\n\n{viewResponse.Content}");
            }

            var paramsString = viewLink.Attributes["href"].Value.TrimStart('#');

            foreach (var param in paramsString.Split('&'))
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
