using CatalogueScanner.Core.Dto.FunctionResult;
using CatalogueScanner.WebScraping.Service;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CatalogueScanner.WebScraping.Functions
{
    public class CheckColesOnlineSpecials
    {
        private readonly ColesOnlineService colesOnlineService;

        public CheckColesOnlineSpecials(ColesOnlineService colesOnlineService)
        {
            this.colesOnlineService = colesOnlineService;
        }

        [FunctionName("CheckColesOnlineSpecials")]
        [return: Queue("coles-online-specials")]
        public async Task<Catalogue> Run([TimerTrigger("0 */10 * * * *")] TimerInfo timer, ILogger log)
        {
            var specialsResult = await colesOnlineService.GetColesOnlineSpecials().ConfigureAwait(false);

            var productUrlTemplate = specialsResult.ProductUrlTemplate;

            var specialsResetTime = colesOnlineService.SpecialsResetTime;

            var now = DateTimeOffset.UtcNow;
            var specialsStartDate = specialsResetTime.GetPreviousDate(now);
            var specialsEndDate = specialsResetTime.GetNextDate(now);

            var items = specialsResult.Data.Products
                .Select(product => new CatalogueItem
                {
                    Id = product.UniqueId,
                    Name = $"{product.Manufacturer} {product.Name}",
                    Sku = product.SingleSkuCatalogEntryId,
                    Uri = new Uri(productUrlTemplate.AbsoluteUri.Replace("[productToken]", product.SeoToken, StringComparison.OrdinalIgnoreCase)),
                })
                .ToList();

            Catalogue catalogue = new Catalogue("Coles Online", specialsStartDate, specialsEndDate, items);
            return catalogue;
        }
    }
}
