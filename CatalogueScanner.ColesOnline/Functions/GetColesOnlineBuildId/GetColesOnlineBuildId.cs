using CatalogueScanner.ColesOnline.Service;
using Microsoft.Azure.Functions.Worker;

namespace CatalogueScanner.ColesOnline.Functions;

public class GetColesOnlineBuildId(ColesOnlineService colesOnlineService)
{
    private readonly ColesOnlineService colesOnlineService = colesOnlineService;

    [Function(ColesOnlineFunctionNames.GetColesOnlineBuildId)]
    public async Task<string> Run(
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Required by Azure Functions")]
        [ActivityTrigger]
        object? input,
        CancellationToken cancellationToken
    )
    {                       
        var response = await colesOnlineService.GetBuildId(cancellationToken).ConfigureAwait(false);

        return response;
    }
}
