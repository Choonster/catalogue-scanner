using CatalogueScanner.ColesOnline.Service;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;

namespace CatalogueScanner.ColesOnline.Functions
{
    public class GetColesOnlineBuildId
    {
        private readonly ColesOnlineService colesOnlineService;

        public GetColesOnlineBuildId(ColesOnlineService colesOnlineService)
        {
            this.colesOnlineService = colesOnlineService;
        }

        // TODO: Might not be able to use TaskActivityContext here
        [Function(ColesOnlineFunctionNames.GetColesOnlineBuildId)]
        public async Task<string> Run([ActivityTrigger] TaskActivityContext context, CancellationToken cancellationToken)
        {
            #region null checks
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            #endregion
                       
            var response = await colesOnlineService.GetBuildId(cancellationToken).ConfigureAwait(false);

            return response;
        }
    }
}
