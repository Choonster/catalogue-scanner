using CatalogueScanner.WebScraping.Service;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
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

        // TODO: Might not be able to use TaskActivityContext here
        [Function(WebScrapingFunctionNames.ClosePlaywrightBrowser)]
        public async Task<bool> Run([ActivityTrigger] TaskActivityContext context, CancellationToken cancellationToken)
        {
            #region null checks
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            #endregion

            return await playwrightBrowserManager.CloseBrowserAsync(context.InstanceId, cancellationToken).ConfigureAwait(false);
        }
    }
}
