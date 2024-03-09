using CatalogueScanner.WoolworthsOnline.Service;
using Microsoft.Azure.Functions.Worker;
using System.Net;

namespace CatalogueScanner.WoolworthsOnline.Functions;

public class GetWoolworthsOnlineCookies(WoolworthsOnlineService woolworthsOnlineService)
{
    private readonly WoolworthsOnlineService woolworthsOnlineService = woolworthsOnlineService;

    [Function(WoolworthsOnlineFunctionNames.GetWoolworthsOnlineCookies)]
    public async Task<CookieCollection> Run(
        CancellationToken cancellationToken,
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Required by Azure Functions")]
        [ActivityTrigger]
        object? input = null
    )
    {
        var cookies = await woolworthsOnlineService.GetCookiesAsync(cancellationToken).ConfigureAwait(false);

        return cookies;
    }
}
