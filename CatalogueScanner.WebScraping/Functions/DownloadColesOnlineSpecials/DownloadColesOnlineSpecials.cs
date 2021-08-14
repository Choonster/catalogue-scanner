using CatalogueScanner.Core.Dto.FunctionResult;
using CatalogueScanner.Core.Utility;
using CatalogueScanner.WebScraping.Service;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System;
using System.Linq;
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

        [FunctionName(WebScrapingFunctionNames.DownloadColesOnlineSpecials)]
        public async Task<Catalogue> Run([ActivityTrigger] DateRange specialsDateRange)
        {
            var specialsResult = await colesOnlineService.GetColesOnlineSpecials().ConfigureAwait(false);

            var productUrlTemplate = specialsResult.ProductUrlTemplate;

            var items = specialsResult.Data.Products
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
