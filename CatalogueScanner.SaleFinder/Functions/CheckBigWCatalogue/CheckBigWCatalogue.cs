using CatalogueScanner.SaleFinder.Dto.FunctionResult;
using CatalogueScanner.SaleFinder.Dto.SaleFinder;
using CatalogueScanner.SaleFinder.Service;
using HtmlAgilityPack;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace CatalogueScanner.SaleFinder.Functions
{
    /// <summary>
    /// Checks for new Big W catalogues and queues them for scanning.
    /// </summary>
    public class CheckBigWCatalogue
    {
        private const int BigWStoreId = 128;
        private const int BigWLocationId = -1; // Big W doesn't seem to use location IDs
        private const string BigWStoreName = "Big W";
        private static readonly Uri CatalaogueBaseUri = new("https://www.bigw.com.au/bigw-catalogues");

        private readonly SaleFinderService saleFinderService;
        private readonly IStringLocalizer<CheckBigWCatalogue> S;

        public CheckBigWCatalogue(SaleFinderService saleFinderService, IStringLocalizer<CheckBigWCatalogue> stringLocalizer)
        {
            this.saleFinderService = saleFinderService ?? throw new ArgumentNullException(nameof(saleFinderService));
            S = stringLocalizer ?? throw new ArgumentNullException(nameof(stringLocalizer));
        }

        [FunctionName(SaleFinderFunctionNames.CheckBigWCatalogue)]
        public async Task RunAsync(
            [TimerTrigger("%" + SaleFinderAppSettingNames.CheckCatalogueFunctionCronExpression + "%")] TimerInfo timer,
            ILogger log,
            [Queue(SaleFinderQueueNames.SaleFinderCataloguesToScan)] IAsyncCollector<SaleFinderCatalogueDownloadInformation> collector
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

            var viewResponse = await saleFinderService.GetCatalogueViewDataAsync(BigWStoreId, BigWLocationId).ConfigureAwait(false);

            if (viewResponse is null)
            {
                throw new InvalidOperationException("viewResponse is null");
            }

            var saleIds = FindSaleIds(viewResponse).ToList();

            log.LogInformation(S["Found sale IDs: {0}"], saleIds);

            foreach (var saleId in saleIds)
            {
                await collector.AddAsync(new SaleFinderCatalogueDownloadInformation(saleId, CatalaogueBaseUri, BigWStoreName)).ConfigureAwait(false);
            }
        }

        private IEnumerable<int> FindSaleIds(CatalogueViewResponse viewResponse)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(viewResponse.Content);

            var viewLinks = doc.DocumentNode
                .Descendants("a")
                .Where(node => node.Attributes["href"] != null)
                .Where(node => node.HasClass("readbutton"))
                .ToList();

            if (!viewLinks.Any())
            {
                throw new UnableToFindSaleIdException($"{S["Didn't find .readbutton links in HTML content."]}\n\n{viewResponse.Content}");
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
}
