using CatalogueScanner.WoolworthsOnline.Service;
using Microsoft.Azure.Functions.Worker;

namespace CatalogueScanner.WoolworthsOnline.Functions;

public class GetWoolworthsOnlineSpecialsPageCount(WoolworthsOnlineService woolworthsOnlineService)
{
    private readonly WoolworthsOnlineService woolworthsOnlineService = woolworthsOnlineService;

    [Function(WoolworthsOnlineFunctionNames.GetWoolworthsOnlineSpecialsPageCount)]
    public async Task<int> Run([ActivityTrigger] string categoryId, CancellationToken cancellationToken)
    {
        #region null checks
        if (string.IsNullOrEmpty(categoryId))
        {
            throw new ArgumentException($"'{nameof(categoryId)}' cannot be null or empty.", nameof(categoryId));
        }
        #endregion

        return await woolworthsOnlineService.GetCategoryPageCountAsync(
            categoryId,
            WoolworthsOnlineService.MaxBrowseCategoryDataPageSize,
            cancellationToken
        ).ConfigureAwait(false);
    }
}
