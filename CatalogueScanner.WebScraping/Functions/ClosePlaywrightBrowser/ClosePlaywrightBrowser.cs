using CatalogueScanner.WebScraping.Service;
using Microsoft.Azure.Functions.Worker;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CatalogueScanner.WebScraping.Functions
{
    public class ClosePlaywrightBrowser
    {
        private readonly PlaywrightBrowserManager playwrightBrowserManager;

        public ClosePlaywrightBrowser(PlaywrightBrowserManager playwrightBrowserManager)
        {
            this.playwrightBrowserManager = playwrightBrowserManager;
        }

        [Function(WebScrapingFunctionNames.ClosePlaywrightBrowser)]
        public async Task<bool> Run([ActivityTrigger] object? _, string instanceId, CancellationToken cancellationToken)
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
}
