using CatalogueScanner.ColesOnline.Service;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace CatalogueScanner.ColesOnline.Functions
{
    public class GetColesOnlineSpecialsPageCount
    {
        private readonly ColesOnlineService colesOnlineService;

        public GetColesOnlineSpecialsPageCount(ColesOnlineService colesOnlineService)
        {
            this.colesOnlineService = colesOnlineService;
        }

        [FunctionName(ColesOnlineFunctionNames.GetColesOnlineSpecialsPageCount)]
        public async Task<int> Run([ActivityTrigger] IDurableActivityContext context, CancellationToken cancellationToken)
        {
            #region null checks
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            #endregion

            return await colesOnlineService.GetOnSpecialPageCountAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
