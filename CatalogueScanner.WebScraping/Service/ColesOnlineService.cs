using CatalogueScanner.Core.Utility;
using CatalogueScanner.WebScraping.Common.Dto.ColesOnline;
using CatalogueScanner.WebScraping.JavaScript;
using CatalogueScanner.WebScraping.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CatalogueScanner.WebScraping.Service
{
    public class ColesOnlineService
    {
        private const string ColesBaseUrl = "https://shop.coles.com.au";

        private readonly ColesOnlineOptions options;
        private readonly ILogger<ColesOnlineService> logger;
        private readonly PlaywrightBrowserManager browserManager;

        public ColesOnlineService(IOptionsSnapshot<ColesOnlineOptions> optionsAccessor, ILogger<ColesOnlineService> logger, PlaywrightBrowserManager browserManager)
        {
            #region null checks
            if (optionsAccessor is null)
            {
                throw new ArgumentNullException(nameof(optionsAccessor));
            }
            #endregion

            options = optionsAccessor.Value;
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.browserManager = browserManager ?? throw new ArgumentNullException(nameof(browserManager));

            #region options null checks
            if (options.StoreId is null)
            {
                throw new ArgumentException($"{ColesOnlineOptions.ColesOnline}.{nameof(options.StoreId)} is null", nameof(optionsAccessor));
            }
            #endregion
        }

        /// <summary>
        /// The time of week when Coles Online changes its specials.
        /// </summary>
        public static TimeOfWeek SpecialsResetTime => new(TimeSpan.Zero, DayOfWeek.Wednesday, "AUS Eastern Standard Time");

        public Uri ProductUrlTemplate => new($"{ColesBaseUrl}/a/{options.StoreId}/product/[productToken]");

        public async Task<int> GetSpecialsTotalPageCountAsync(string instanceId, CancellationToken cancellationToken = default)
        {
            var context = await browserManager.NewContextAsync(instanceId, cancellationToken: cancellationToken).ConfigureAwait(false);
            await using var _ = context.ConfigureAwait(false);

            var page = await CreateAndInitialiseSpecialsPage(context).ConfigureAwait(false);

            await page.WaitForFunctionAsync("CatalogueScanner_ColesOnline.instance.isPaginationLoaded").ConfigureAwait(false);

            return await page.EvaluateAsync<int>("CatalogueScanner_ColesOnline.instance.totalPageCount").ConfigureAwait(false);
        }

        public async Task<ColrsCatalogEntryList> GetSpecialsPageAsync(string instanceId, int pageNum, CancellationToken cancellationToken = default)
        {
            var context = await browserManager.NewContextAsync(instanceId, cancellationToken: cancellationToken).ConfigureAwait(false);
            await using var _ = context.ConfigureAwait(false);

            var page = await CreateAndInitialiseSpecialsPage(context).ConfigureAwait(false);

            logger.LogWarning("Page {PageNum} - Starting", pageNum);

            await page.WaitForFunctionAsync("CatalogueScanner_ColesOnline.instance.isPaginationLoaded").ConfigureAwait(false);

            await page.EvaluateAsync("pageNum => CatalogueScanner_ColesOnline.instance.loadPage(pageNum)", pageNum).ConfigureAwait(false);

            // The server returns the JSON data in compressed form with keys like "p1" and "a" that then get converted to full keys like "name" and "attributesMap".
            // Wait until the data has been decompressed before we read it.
            await page.WaitForFunctionAsync("CatalogueScanner_ColesOnline.instance.isDataLoaded").ConfigureAwait(false);

            logger.LogWarning("Page {PageNum} - Data Loaded", pageNum);

            // Playwright's EvaluateArgumentValueConverter doesn't seem to be able to deserialise to ColrsCatalogEntryList (and doesn't handle custom names),
            // so serialise the data to JSON in the browser and then deserialise to ColrsCatalogEntryList (using Newtonsoft.Json rather than System.Text.Json).
            var productDataJson = await page.EvaluateAsync<string>("JSON.stringify(CatalogueScanner_ColesOnline.instance.productListData)")
                                            .ConfigureAwait(false);

            if (productDataJson is null)
            {
                throw new InvalidOperationException($"{nameof(productDataJson)} is null after evaluating productListData expression");
            }

            logger.LogWarning("Page {PageNum} - Data Received from Playwright", pageNum);

            var productData = JsonConvert.DeserializeObject<ColrsCatalogEntryList>(productDataJson);

            if (productData is null)
            {
                throw new InvalidOperationException($"{nameof(productData)} is null after deserialising from {nameof(productDataJson)}");
            }

            return productData;
        }

        private async Task<IPage> CreateAndInitialiseSpecialsPage(IBrowserContext context)
        {
            var page = await context.NewPageAsync().ConfigureAwait(false);

            page.SetDefaultTimeout(0);

            await page.AddInitScriptAsync(script: JavaScriptFiles.ColesOnline).ConfigureAwait(false);

            // Prevent all external requests for advertising, tracking, etc.
            await page.RouteAsync(
                (url) => !url.StartsWith(ColesBaseUrl, StringComparison.OrdinalIgnoreCase),
                (route) => route.AbortAsync()
            ).ConfigureAwait(false);

            // Navigate to the Specials page
            var response = await page.GotoAsync(
                $"{ColesBaseUrl}/a/{options.StoreId}/specials/browse",
                new PageGotoOptions
                {
                    WaitUntil = WaitUntilState.DOMContentLoaded,
                }
            ).ConfigureAwait(false);

            if (response is null)
            {
                throw new InvalidOperationException("Page goto response is null");
            }

            if (!response.Ok)
            {
                throw new HttpRequestException($"Coles Online request failed with status {response.Status} ({response.StatusText}) - {await response.TextAsync().ConfigureAwait(false)}");
            }

            return page;
        }
    }
}
