using CatalogueScanner.Dto.Config;
using CatalogueScanner.Dto.SaleFinder;
using CatalogueScanner.SharedCode.Dto.StorageEntity;
using HtmlAgilityPack;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace CatalogueScanner
{
    /// <summary>
    /// Checks for new Coles catalogues and queues them for download.
    /// </summary>
    public class CheckColesCatalogue
    {
        private const int ColesStoreId = 148;

        private static readonly Uri CatalagoueBaseUri = new Uri("https://www.coles.com.au/catalogues-and-specials/view-all-available-catalogues");

        private readonly SaleFinderService saleFinderService;
        private readonly ColesSettings settings;

        public CheckColesCatalogue(SaleFinderService saleFinderService, IOptionsSnapshot<CatalogueScannerSettings> settings)
        {
            #region null checks
            if (settings is null)
            {
                throw new ArgumentNullException(nameof(settings));
            }
            #endregion

            this.saleFinderService = saleFinderService ?? throw new ArgumentNullException(nameof(saleFinderService));
            this.settings = settings.Value.Coles;
        }

        [FunctionName("CheckColesCatalogue")]
        [return: Queue(Constants.QueueNames.SaleFinderCataloguesToDownload)]
        public async Task<SaleFinderCatalogueDownloadInformation> RunAsync(
            [TimerTrigger("0 */5 * * * *")]
            TimerInfo myTimer,
            ILogger log
        )
        {
            #region null checks
            if (myTimer is null)
            {
                throw new ArgumentNullException(nameof(myTimer));
            }

            if (log is null)
            {
                throw new ArgumentNullException(nameof(log));
            }
            #endregion

            var viewResponse = await saleFinderService.GetCatalogueViewDataAsync(ColesStoreId, settings.SaleFinderLocationId).ConfigureAwait(false);

            var saleId = FindSaleId(viewResponse);

            log.LogInformation($"Found sale ID: {saleId}");

            return new SaleFinderCatalogueDownloadInformation
            {
                SaleId = saleId,
                BaseUri = CatalagoueBaseUri,
            };
        }

        private static int FindSaleId(CatalogueViewResponse viewResponse)
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
                throw new UnableToFindSaleIdException("Didn't find \"This week's catalogue\" cell in HTML content.\n\n" + viewResponse.Content);
            }

            var viewLink = thisWeeksCatalogueHeader.ParentNode
                .Descendants("a")
                .Where(node => node.Attributes["href"] != null)
                .Where(node => node.HasClass("sf-view-button"))
                .Where(node => node.HasClass("button-secondary"))
                .FirstOrDefault();

            if (viewLink is null)
            {
                throw new UnableToFindSaleIdException("Didn't find \"View\" link in HTML content.\n\n" + viewResponse.Content);
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

            throw new UnableToFindSaleIdException("Didn't find saleId parameter in View link in HTML content.\n\n" + viewResponse.Content);
        }
    }
}
