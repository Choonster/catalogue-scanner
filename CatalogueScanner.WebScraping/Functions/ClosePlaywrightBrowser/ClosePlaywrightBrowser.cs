using CatalogueScanner.WebScraping.Service;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
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

        [FunctionName(WebScrapingFunctionNames.ClosePlaywrightBrowser)]
        public async Task<bool> Run([ActivityTrigger] IDurableActivityContext context, CancellationToken cancellationToken)
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
