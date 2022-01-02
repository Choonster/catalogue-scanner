using CatalogueScanner.Core.Dto.FunctionResult;
using CatalogueScanner.WebScraping.Service;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CatalogueScanner.WebScraping.Functions
{
    public class DownloadColesOnlineSpecialsPage
    {
        private readonly ColesOnlineService colesOnlineService;

        public DownloadColesOnlineSpecialsPage(ColesOnlineService colesOnlineService)
        {
            this.colesOnlineService = colesOnlineService;
        }

        [Timeout("00:10:00")]
        [FunctionName(WebScrapingFunctionNames.DownloadColesOnlineSpecialsPage)]
        public async Task<IEnumerable<CatalogueItem>> Run([ActivityTrigger] IDurableActivityContext context, CancellationToken cancellationToken)
        {
            #region null checks
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            #endregion

            cancellationToken.ThrowIfCancellationRequested();

            var pageNum = context.GetInput<int>();

            var productUrlTemplate = colesOnlineService.ProductUrlTemplate;
            var specials = await colesOnlineService.GetSpecialsPageAsync(context.InstanceId, pageNum, cancellationToken).ConfigureAwait(false);

            var items = specials.Products
                .Select(product => new CatalogueItem
                {
                    Id = product.UniqueId,
                    Name = $"{product.Manufacturer} {product.Name}",
                    Sku = product.SingleSkuCatalogEntryId,
                    Uri = new Uri(productUrlTemplate.AbsoluteUri.Replace("[productToken]", product.SeoToken, StringComparison.OrdinalIgnoreCase)),
                })
                .ToList();

            return items;
        }
    }
}
