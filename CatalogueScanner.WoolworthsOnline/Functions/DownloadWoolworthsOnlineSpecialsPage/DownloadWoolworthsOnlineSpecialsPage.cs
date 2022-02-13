using CatalogueScanner.Core.Dto.FunctionResult;
using CatalogueScanner.WoolworthsOnline.Dto.FunctionInput;
using CatalogueScanner.WoolworthsOnline.Dto.WoolworthsOnline;
using CatalogueScanner.WoolworthsOnline.Service;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CatalogueScanner.WoolworthsOnline.Functions
{
    public class DownloadWoolworthsOnlineSpecialsPage
    {
        private readonly WoolworthsOnlineService woolworthsOnlineService;

        public DownloadWoolworthsOnlineSpecialsPage(WoolworthsOnlineService woolworthsOnlineService)
        {
            this.woolworthsOnlineService = woolworthsOnlineService;
        }

        [FunctionName(WoolworthsOnlineFunctionNames.DownloadWoolworthsOnlineSpecialsPage)]
        public async Task<IEnumerable<CatalogueItem>> Run([ActivityTrigger] IDurableActivityContext context, CancellationToken cancellationToken)
        {
            #region null checks
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            #endregion

            var input = context.GetInput<DownloadWoolworthsOnlineSpecialsPageInput>();

            if (input is null)
            {
                throw new InvalidOperationException("Activity function input not present");
            }

            var productUrlTemplate = WoolworthsOnlineService.ProductUrlTemplate;

            var response = await woolworthsOnlineService.GetBrowseCategoryDataAsync(new BrowseCategoryRequest
            {
                CategoryId = input.CategoryId,
                PageNumber = input.PageNumber,
                PageSize = WoolworthsOnlineService.MaxBrowseCategoryDataPageSize,
            }, cancellationToken).ConfigureAwait(false);

            var items = response.Bundles
               .SelectMany(bundle =>
                    bundle.Products.Select(product =>
                        new CatalogueItem
                        {
                            Id = product.Stockcode.ToString(CultureInfo.InvariantCulture),
                            Name = product.DisplayName,
                            Sku = product.Stockcode.ToString(CultureInfo.InvariantCulture),
                            Uri = new Uri(productUrlTemplate.AbsoluteUri.Replace("[stockCode]", product.Stockcode.ToString(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase)),
                        }
                    )
                )
               .ToList();

            return items;
        }
    }
}
