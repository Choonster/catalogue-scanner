using CatalogueScanner.ColesOnline.Dto.FunctionInput;
using CatalogueScanner.ColesOnline.Service;
using Microsoft.Azure.Functions.Worker;

namespace CatalogueScanner.ColesOnline.Functions
{
    public class GetColesOnlineSpecialsPageCount
    {
        private readonly ColesOnlineService colesOnlineService;

        public GetColesOnlineSpecialsPageCount(ColesOnlineService colesOnlineService)
        {
            this.colesOnlineService = colesOnlineService;
        }

        [Function(ColesOnlineFunctionNames.GetColesOnlineSpecialsPageCount)]
        public async Task<int> Run([ActivityTrigger] GetColesOnlineSpecialsPageCountInput input, CancellationToken cancellationToken)
        {
            #region null checks
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input));
            }
            #endregion

            return await colesOnlineService.GetOnSpecialPageCountAsync(input.BuildId, cancellationToken).ConfigureAwait(false);
        }
    }
}
