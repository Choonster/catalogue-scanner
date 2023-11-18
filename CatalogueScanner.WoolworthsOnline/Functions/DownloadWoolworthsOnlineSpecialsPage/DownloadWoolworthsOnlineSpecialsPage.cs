using CatalogueScanner.Core.Dto.FunctionResult;
using CatalogueScanner.WoolworthsOnline.Dto.FunctionInput;
using CatalogueScanner.WoolworthsOnline.Dto.WoolworthsOnline;
using CatalogueScanner.WoolworthsOnline.Service;
using Microsoft.Azure.Functions.Worker;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CatalogueScanner.WoolworthsOnline.Functions;

public class DownloadWoolworthsOnlineSpecialsPage(WoolworthsOnlineService woolworthsOnlineService)
{
    [Function(WoolworthsOnlineFunctionNames.DownloadWoolworthsOnlineSpecialsPage)]
    public async Task<IEnumerable<CatalogueItem>> Run(
        [ActivityTrigger] DownloadWoolworthsOnlineSpecialsPageInput input,
        CancellationToken cancellationToken
    )
    {
        #region null checks
        ArgumentNullException.ThrowIfNull(input);
        #endregion

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
                    new CatalogueItem(
                        product.Stockcode.ToString(CultureInfo.InvariantCulture),
                        product.DisplayName,
                        product.Stockcode.ToString(CultureInfo.InvariantCulture),
                        new Uri(productUrlTemplate.AbsoluteUri.Replace("[stockCode]", product.Stockcode.ToString(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase)),
                        product.Price,
                        product.CentreTag?.MultibuyData?.Quantity,
                        product.CentreTag?.MultibuyData?.Price
                    )
                )
            )
           .ToList();

        return items;
    }
}
