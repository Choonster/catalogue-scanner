using CatalogueScanner.ColesOnline.Service;
using Microsoft.Azure.Functions.Worker;

namespace CatalogueScanner.ColesOnline.Functions
{
    public class GetColesOnlineBuildId
    {
        private readonly ColesOnlineService colesOnlineService;

        public GetColesOnlineBuildId(ColesOnlineService colesOnlineService)
        {
            this.colesOnlineService = colesOnlineService;
        }

        [Function(ColesOnlineFunctionNames.GetColesOnlineBuildId)]
        public async Task<string> Run([ActivityTrigger] object? _, CancellationToken cancellationToken)
        {                       
            var response = await colesOnlineService.GetBuildId(cancellationToken).ConfigureAwait(false);

            return response;
        }
    }
}
