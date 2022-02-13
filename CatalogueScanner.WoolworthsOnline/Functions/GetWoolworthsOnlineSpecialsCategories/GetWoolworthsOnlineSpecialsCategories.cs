using CatalogueScanner.WoolworthsOnline.Dto.FunctionResult;
using CatalogueScanner.WoolworthsOnline.Dto.WoolworthsOnline;
using CatalogueScanner.WoolworthsOnline.Service;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CatalogueScanner.WoolworthsOnline.Functions
{
    public class GetWoolworthsOnlineSpecialsCategories
    {
        private readonly WoolworthsOnlineService woolworthsOnlineService;

        public GetWoolworthsOnlineSpecialsCategories(WoolworthsOnlineService woolworthsOnlineService)
        {
            this.woolworthsOnlineService = woolworthsOnlineService;
        }

        [FunctionName(WoolworthsOnlineFunctionNames.GetWoolworthsOnlineSpecialsCategories)]
        public async Task<IEnumerable<WoolworthsOnlineCategory>> Run([ActivityTrigger] IDurableActivityContext context, CancellationToken cancellationToken)
        {
            #region null checks
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            #endregion

            var response = await woolworthsOnlineService.GetPiesCategoriesWithSpecialsAsync(cancellationToken).ConfigureAwait(false);

            var specialsCategories = new List<WoolworthsOnlineCategory>();
            
            FilterSpecialsCategories(response.Categories, specialsCategories);

            return specialsCategories;
        }

        private void FilterSpecialsCategories(IEnumerable<Category> categories, List<WoolworthsOnlineCategory> specialsCategories)
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
}
