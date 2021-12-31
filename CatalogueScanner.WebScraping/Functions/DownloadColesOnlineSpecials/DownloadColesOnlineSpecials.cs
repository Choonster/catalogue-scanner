using CatalogueScanner.Core.Dto.FunctionResult;
using CatalogueScanner.Core.Utility;
using CatalogueScanner.WebScraping.Service;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CatalogueScanner.WebScraping.Functions
{
    public class DownloadColesOnlineSpecials
    {
        private readonly ColesOnlineService colesOnlineService;

        public DownloadColesOnlineSpecials(ColesOnlineService colesOnlineService)
        {
            this.colesOnlineService = colesOnlineService;
        }

        [Timeout("-1")]
        [FunctionName(WebScrapingFunctionNames.DownloadColesOnlineSpecials)]
        public async Task<Catalogue> Run([ActivityTrigger] DateRange specialsDateRange, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var productUrlTemplate = colesOnlineService.ProductUrlTemplate;
            var specials = await colesOnlineService.GetSpecialsAsync(cancellationToken).ConfigureAwait(false);

            var items = specials
                .SelectMany(s => s.Products)
                .Select(product => new CatalogueItem
                {
                    Id = product.UniqueId,
                    Name = $"{product.Manufacturer} {product.Name}",
                    Sku = product.SingleSkuCatalogEntryId,
                    Uri = new Uri(productUrlTemplate.AbsoluteUri.Replace("[productToken]", product.SeoToken, StringComparison.OrdinalIgnoreCase)),
                })
                .ToList();

            return new Catalogue("Coles Online", specialsDateRange.StartDate, specialsDateRange.EndDate, items);
        }
    }
}
