using CatalogueScanner.WebScraping.Service;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CatalogueScanner.WebScraping.Functions
{
    public class GetColesOnlineSpecialsPageCount
    {
        private readonly ColesOnlineService colesOnlineService;

        public GetColesOnlineSpecialsPageCount(ColesOnlineService colesOnlineService)
        {
            this.colesOnlineService = colesOnlineService;
        }

        [FunctionName(WebScrapingFunctionNames.GetColesOnlineSpecialsPageCount)]
        public async Task<int> Run([ActivityTrigger] IDurableActivityContext context, CancellationToken cancellationToken)
        {
            #region null checks
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            #endregion

            return await colesOnlineService.GetSpecialsTotalPageCountAsync(context.InstanceId, cancellationToken).ConfigureAwait(false);
        }
    }
}
