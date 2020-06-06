using CatalogueScanner.SaleFinder.DTO.Config;
using CatalogueScanner.SaleFinder.DTO.FunctionResult;
using CatalogueScanner.SaleFinder.DTO.SaleFinder;
using CatalogueScanner.SaleFinder.Service;
using HtmlAgilityPack;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CatalogueScanner.SaleFinder.Functions.CheckWoolworthsCatalogue
{
    public class CheckWoolworthsCatalogue
    {
        private const int WoolworthsStoreId = 126;
        private const string WoolworthsStoreName = "Woolworths";
        private const string WeeklySpecialsCatalogue = "Weekly Specials Catalogue";
        private static readonly Uri CatalaogueBaseUri = new Uri("https://www.woolworths.com.au/shop/catalogue");

        private readonly SaleFinderService saleFinderService;
        private readonly WoolworthsSettings settings;
        private readonly IStringLocalizer<CheckWoolworthsCatalogue> S;

        public CheckWoolworthsCatalogue(SaleFinderService saleFinderService, IOptionsSnapshot<WoolworthsSettings> settings, IStringLocalizer<CheckWoolworthsCatalogue> stringLocalizer)
        {
            #region null checks
            if (settings is null)
            {
                throw new ArgumentNullException(nameof(settings));
            }
            #endregion

            this.saleFinderService = saleFinderService ?? throw new ArgumentNullException(nameof(saleFinderService));
            this.settings = settings.Value;
            S = stringLocalizer ?? throw new ArgumentNullException(nameof(stringLocalizer));
        }

        [FunctionName(SaleFinderConstants.FunctionNames.CheckWoolworthsCatalogue)]
        [return: Queue(SaleFinderConstants.QueueNames.SaleFinderCataloguesToScan)]
        public async Task<SaleFinderCatalogueDownloadInformation> RunAsync(
            [TimerTrigger("%" + SaleFinderConstants.AppSettingNames.CheckCatalogueFunctionCronExpression + "%")] TimerInfo timer,
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

            var viewResponse = await saleFinderService.GetCatalogueViewDataAsync(WoolworthsStoreId, settings.SaleFinderLocationId).ConfigureAwait(false);

            var saleId = FindSaleId(viewResponse);

            log.LogInformation(S["Found sale ID: {0}", saleId]);

            return new SaleFinderCatalogueDownloadInformation(saleId, CatalaogueBaseUri, WoolworthsStoreName);
        }

        private int FindSaleId(CatalogueViewResponse viewResponse)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(viewResponse.Content);

            var weeklySpecialsCatalogueContainer = doc.DocumentNode
                .Descendants("div")
                .Where(node => node.HasClass("sale-container"))
                .Where(node => node.GetDataAttribute("saleid") != null)
                .Where(node => node.GetDataAttribute("salename")?.DeEntitizeValue.Contains(WeeklySpecialsCatalogue, StringComparison.InvariantCultureIgnoreCase) ?? false)
                .FirstOrDefault();

            if (weeklySpecialsCatalogueContainer is null)
            {
                throw new UnableToFindSaleIdException($"{S["Didn't find \"{0}\" container in HTML content.", WeeklySpecialsCatalogue]}\n\n{viewResponse.Content}");
            }

            var saleIdValue = weeklySpecialsCatalogueContainer.GetDataAttribute("saleid").Value;
            if (!int.TryParse(saleIdValue, out var saleId))
            {
                throw new UnableToFindSaleIdException($"{S["Found invalid \"{0}\" attribute value \"{1}\" in HTML content.", "data-saleid", saleIdValue]}\n\n{viewResponse.Content}");
            }

            return saleId;
        }
    }
}
