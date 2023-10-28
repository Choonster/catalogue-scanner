using CatalogueScanner.Core;
using CatalogueScanner.SaleFinder.Dto.FunctionResult;
using CatalogueScanner.SaleFinder.Dto.SaleFinder;
using CatalogueScanner.SaleFinder.Options;
using CatalogueScanner.SaleFinder.Service;
using HtmlAgilityPack;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CatalogueScanner.SaleFinder.Functions
{
    /// <summary>
    /// Checks for new IGA catalogues and queues them for scanning.
    /// </summary>
    public class CheckIgaCatalogue
    {
        private const int IgaStoreId = 183;
        private const string IgaStoreName = "IGA";
        private static readonly Uri CatalaogueBaseUri = new("https://www.iga.com.au/catalogue/");

        private readonly SaleFinderService saleFinderService;
        private readonly IgaOptions options;
        private readonly ILogger<CheckIgaCatalogue> logger;
        private readonly IStringLocalizer<CheckIgaCatalogue> S;

        public CheckIgaCatalogue(SaleFinderService saleFinderService, IOptionsSnapshot<IgaOptions> optionsAccessor, ILogger<CheckIgaCatalogue> logger, IStringLocalizer<CheckIgaCatalogue> stringLocalizer)
        {
            #region null checks
            if (optionsAccessor is null)
            {
                throw new ArgumentNullException(nameof(optionsAccessor));
            }
            #endregion

            this.saleFinderService = saleFinderService ?? throw new ArgumentNullException(nameof(saleFinderService));
            options = optionsAccessor.Value;
            this.logger = logger;
            S = stringLocalizer ?? throw new ArgumentNullException(nameof(stringLocalizer));
        }

        [Function(SaleFinderFunctionNames.CheckIgaCatalogue)]
        [QueueOutput(SaleFinderQueueNames.SaleFinderCataloguesToScan)]
        public async Task<SaleFinderCatalogueDownloadInformation[]> RunAsync(
            [TimerTrigger("%" + SaleFinderAppSettingNames.CheckCatalogueFunctionCronExpression + "%")] TimerInfo timer,
            CancellationToken cancellationToken
        )
        {
            #region null checks
            if (timer is null)
            {
                throw new ArgumentNullException(nameof(timer));
            }
            #endregion

            var viewResponse = await saleFinderService.GetCatalogueViewDataAsync(IgaStoreId, options.SaleFinderLocationId, cancellationToken).ConfigureAwait(false);

            if (viewResponse is null)
            {
                throw new InvalidOperationException("viewResponse is null");
            }

            var saleIds = FindSaleIds(viewResponse).ToList();

            logger.LogInformation(S["Found sale IDs: {0}"], saleIds);

            return saleIds
                .Select(saleId => new SaleFinderCatalogueDownloadInformation(saleId, CatalaogueBaseUri, IgaStoreName, CurrencyCultures.AustralianDollar))
                .ToArray();
        }

        private IEnumerable<int> FindSaleIds(CatalogueViewResponse viewResponse)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(viewResponse.Content);

            var viewLinks = doc.DocumentNode
                .Descendants("a")
                .Where(node => node.Attributes["href"] != null)
                .Where(node => node.HasClass("sf-readcatalogue-btn"))
                .ToList();

            if (!viewLinks.Any())
            {
                throw new UnableToFindSaleIdException($"{S["Didn't find .sf-readcatalogue-btn links in HTML content."]}\n\n{viewResponse.Content}");
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
