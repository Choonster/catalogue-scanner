using CatalogueScanner.WoolworthsOnline.Dto.FunctionInput;
using CatalogueScanner.WoolworthsOnline.Service;
using Microsoft.Azure.Functions.Worker;

namespace CatalogueScanner.WoolworthsOnline.Functions;

public class GetWoolworthsOnlineSpecialsPageCount(WoolworthsOnlineService woolworthsOnlineService)
{
    private readonly WoolworthsOnlineService woolworthsOnlineService = woolworthsOnlineService;

    [Function(WoolworthsOnlineFunctionNames.GetWoolworthsOnlineSpecialsPageCount)]
    public async Task<int> Run([ActivityTrigger] GetWoolworthsOnlineSpecialsPageCountInput input, CancellationToken cancellationToken)
    {
        #region null checks
        ArgumentNullException.ThrowIfNull(input);

        if (string.IsNullOrEmpty(input.CategoryId))
        {
            throw new ArgumentException($"'{nameof(input)}.{nameof(input.CategoryId)}' cannot be null or empty.", nameof(input));
        }
        #endregion

        return await woolworthsOnlineService.GetCategoryPageCountAsync(
            input.CategoryId,
            WoolworthsOnlineService.MaxBrowseCategoryDataPageSize,
            input.Cookies,
            cancellationToken
        ).ConfigureAwait(false);
    }
}
