using CatalogueScanner.Core.Utility;
using CatalogueScanner.WebScraping.Common.Dto.ColesOnline;
using CatalogueScanner.WebScraping.JavaScript;
using CatalogueScanner.WebScraping.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace CatalogueScanner.WebScraping.Service
{
    public class ColesOnlineService
    {
        private const string ColesBaseUrl = "https://shop.coles.com.au";

        private readonly ColesOnlineOptions options;
        private readonly ILogger<ColesOnlineService> logger;

        public ColesOnlineService(IOptionsSnapshot<ColesOnlineOptions> optionsAccessor, ILogger<ColesOnlineService> logger)
        {
            #region null checks
            if (optionsAccessor is null)
            {
                throw new ArgumentNullException(nameof(optionsAccessor));
            }
            #endregion

            options = optionsAccessor.Value;
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

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

        public async Task<IEnumerable<ColrsCatalogEntryList>> GetSpecialsAsync()
        {
            using var playwright = await Playwright.CreateAsync().ConfigureAwait(false);

            var browser = await playwright.Chromium.LaunchAsync().ConfigureAwait(false);
            await using var _ = browser.ConfigureAwait(false);

            var page = await browser.NewPageAsync().ConfigureAwait(false);

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
                    Timeout = 0
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

            var result = new List<ColrsCatalogEntryList>();

            await page.WaitForFunctionAsync("CatalogueScanner_ColesOnline.instance.isPaginationLoaded").ConfigureAwait(false);

            var currentPageNum = await GetCurrentPageNum().ConfigureAwait(false);
            var totalPageCount = await page.EvaluateAsync<int>("CatalogueScanner_ColesOnline.instance.totalPageCount").ConfigureAwait(false);

            logger.LogWarning("Scanning {TotalPageCount} pages of Coles Online specials", totalPageCount);

            while (currentPageNum <= totalPageCount)
            {
                // The server returns the JSON data in compressed form with keys like "p1" and "a" that then get converted to full keys like "name" and "attributesMap".
                // Wait until the data has been decompressed before we read it.
                await page.WaitForFunctionAsync(
                    "CatalogueScanner_ColesOnline.instance.isDataLoaded",
                    options: new PageWaitForFunctionOptions { Timeout = 0 }
                ).ConfigureAwait(false);

                // Playwright's EvaluateArgumentValueConverter doesn't seem to be able to deserialise to ColrsCatalogEntryList (and doesn't handle custom names),
                // so evaluate the expression as a JsonElement and then re-serialise and deserialise to ColrsCatalogEntryList (using Newtonsoft.Json rather than System.Text.Json).
                var productDataJson = await page.EvaluateAsync<JsonElement>("CatalogueScanner_ColesOnline.instance.productListData")
                                                .ConfigureAwait(false);

                var productData = JsonConvert.DeserializeObject<ColrsCatalogEntryList>(productDataJson.GetRawText());

                if (productData is null)
                {
                    throw new InvalidOperationException("productData is null after deserialising from JsonElement.GetRawText()");
                }

                result.Add(productData);

                await page.EvaluateAsync("CatalogueScanner_ColesOnline.instance.nextPage()").ConfigureAwait(false);
                currentPageNum = await GetCurrentPageNum().ConfigureAwait(false);
            }

            return result;

            async Task<int> GetCurrentPageNum() => await page.EvaluateAsync<int>("CatalogueScanner_ColesOnline.instance.currentPageNum").ConfigureAwait(false);
        }
    }
}
