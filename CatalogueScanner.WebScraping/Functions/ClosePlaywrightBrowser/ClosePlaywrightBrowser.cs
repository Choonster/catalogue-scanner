using CatalogueScanner.WebScraping.Service;
using Microsoft.Azure.Functions.Worker;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CatalogueScanner.WebScraping.Functions;

public class ClosePlaywrightBrowser(PlaywrightBrowserManager playwrightBrowserManager)
{
    private readonly PlaywrightBrowserManager playwrightBrowserManager = playwrightBrowserManager;

    [Function(WebScrapingFunctionNames.ClosePlaywrightBrowser)]
    public async Task<bool> Run(
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Required by Azure Functions")]
        [ActivityTrigger] 
        object? input,
        string instanceId,
        CancellationToken cancellationToken
    )
    {
        #region null checks
        if (string.IsNullOrEmpty(instanceId))
        {
            throw new ArgumentException($"'{nameof(instanceId)}' cannot be null or empty.", nameof(instanceId));
        }
        #endregion

        return await playwrightBrowserManager.CloseBrowserAsync(instanceId, cancellationToken).ConfigureAwait(false);
    }
}
