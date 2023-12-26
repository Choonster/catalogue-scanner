using CatalogueScanner.ColesOnline.Dto.FunctionInput;
using CatalogueScanner.ColesOnline.Service;
using Microsoft.Azure.Functions.Worker;

namespace CatalogueScanner.ColesOnline.Functions;

public class GetColesOnlineSpecialsPageCount(ColesOnlineService colesOnlineService)
{
    [Function(ColesOnlineFunctionNames.GetColesOnlineSpecialsPageCount)]
    public async Task<int> Run([ActivityTrigger] GetColesOnlineSpecialsPageCountInput input, CancellationToken cancellationToken)
    {
        #region null checks
        ArgumentNullException.ThrowIfNull(input);
        #endregion

        return await colesOnlineService.GetOnSpecialPageCountAsync(input.BuildId, cancellationToken).ConfigureAwait(false);
    }
}
