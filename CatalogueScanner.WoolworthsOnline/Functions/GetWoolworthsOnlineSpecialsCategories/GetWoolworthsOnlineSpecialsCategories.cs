using CatalogueScanner.WoolworthsOnline.Dto.FunctionResult;
using CatalogueScanner.WoolworthsOnline.Dto.WoolworthsOnline;
using CatalogueScanner.WoolworthsOnline.Service;
using Microsoft.Azure.Functions.Worker;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CatalogueScanner.WoolworthsOnline.Functions;

public class GetWoolworthsOnlineSpecialsCategories(WoolworthsOnlineService woolworthsOnlineService)
{
    private readonly WoolworthsOnlineService woolworthsOnlineService = woolworthsOnlineService;

    [Function(WoolworthsOnlineFunctionNames.GetWoolworthsOnlineSpecialsCategories)]
    public async Task<IEnumerable<WoolworthsOnlineCategory>> Run(
        CancellationToken cancellationToken,
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Required by Azure Functions")]
        [ActivityTrigger]
        object? input = null
    )
    {
        var response = await woolworthsOnlineService.GetPiesCategoriesWithSpecialsAsync(cancellationToken).ConfigureAwait(false);

        var specialsCategories = new List<WoolworthsOnlineCategory>();

        FilterSpecialsCategories(response.Categories, specialsCategories);

        return specialsCategories;
    }

    private static void FilterSpecialsCategories(IEnumerable<Category> categories, List<WoolworthsOnlineCategory> specialsCategories)
    {
        foreach (var category in categories)
        {
            if (category.IsSpecial && category.ProductCount > 0)
            {
                specialsCategories.Add(new WoolworthsOnlineCategory
                {
                    CategoryId = category.NodeId,
                    Description = category.Description,
                });
            }
            else
            {
                FilterSpecialsCategories(category.Children, specialsCategories);
            }
        }
    }
}
