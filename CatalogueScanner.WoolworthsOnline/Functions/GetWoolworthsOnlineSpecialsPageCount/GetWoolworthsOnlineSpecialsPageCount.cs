using CatalogueScanner.WoolworthsOnline.Service;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
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
}
