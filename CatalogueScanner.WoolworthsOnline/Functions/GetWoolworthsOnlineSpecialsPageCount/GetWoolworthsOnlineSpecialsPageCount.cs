using CatalogueScanner.WoolworthsOnline.Service;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CatalogueScanner.WoolworthsOnline.Functions
{
    public class GetWoolworthsOnlineSpecialsPageCount
    {
        private readonly WoolworthsOnlineService woolworthsOnlineService;

        public GetWoolworthsOnlineSpecialsPageCount(WoolworthsOnlineService woolworthsOnlineService)
        {
            this.woolworthsOnlineService = woolworthsOnlineService;
        }

        [FunctionName(WoolworthsOnlineFunctionNames.GetWoolworthsOnlineSpecialsPageCount)]
        public async Task<int> Run([ActivityTrigger] IDurableActivityContext context, CancellationToken cancellationToken)
        {
            #region null checks
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            #endregion

            var categoryId = context.GetInput<string>();

            if (string.IsNullOrEmpty(categoryId))
            {
                throw new InvalidOperationException("Activity function input not present");
            }

            return await woolworthsOnlineService.GetCategoryPageCountAsync(
                categoryId,
                WoolworthsOnlineService.MaxBrowseCategoryDataPageSize,
                cancellationToken
            ).ConfigureAwait(false);
        }
    }
}
