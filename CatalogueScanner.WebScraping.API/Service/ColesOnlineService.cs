using CatalogueScanner.WebScraping.API.Options;
using CatalogueScanner.WebScraping.Common.Dto.ColesOnline;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace CatalogueScanner.WebScraping.API.Service
{
    public class ColesOnlineService
    {
        private const string ColesBaseUrl = "https://shop.coles.com.au";

        private readonly ColesOnlineOptions options;

        public ColesOnlineService(IOptionsSnapshot<ColesOnlineOptions> optionsAccessor)
        {
            #region null checks
            if (optionsAccessor is null)
            {
                throw new ArgumentNullException(nameof(optionsAccessor));
            }
            #endregion

            options = optionsAccessor.Value;

            #region options null checks
            if (options.StoreId is null)
            {
                throw new ArgumentException($"{ColesOnlineOptions.ColesOnline}.{nameof(options.StoreId)} is null", nameof(optionsAccessor));
            }
            #endregion
        }

        public Uri ProductUrlTemplate => new($"{ColesBaseUrl}/a/{options.StoreId}/product/[productToken]");

        public async Task<ColrsCatalogEntryList> GetSpecialsAsync()
        {
            using var playwright = await Playwright.CreateAsync().ConfigureAwait(false);

            var browser = await playwright.Chromium.LaunchAsync().ConfigureAwait(false);
            await using var _ = browser.ConfigureAwait(false);

            var page = await browser.NewPageAsync().ConfigureAwait(false);

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

            const string colrsProductListDataExpression = "angular.element(document.querySelector('[data-colrs-product-list]')).data()?.$colrsProductListController?.widget?.data";

            // The server returns the JSON data in compressed form with keys like "p1" and "a" that then get converted to full keys like "name" and "attributesMap".
            // Wait until the data has been decompressed before we read it.
            await page.WaitForFunctionAsync($"typeof {colrsProductListDataExpression}.products[{colrsProductListDataExpression}.products.length - 1].name === 'string'", options: new PageWaitForFunctionOptions { Timeout = 0 }).ConfigureAwait(false);

            // Playwright's EvaluateArgumentValueConverter doesn't seem to be able to deserialise to ColrsCatalogEntryList (and doesn't handle custom names),
            // so evaluate the expression as a JsonElement and then re-serialise and deserialise to ColrsCatalogEntryList (using Newtonsoft.Json rather than System.Text.Json).
            var productDataJson = await page.EvaluateAsync<JsonElement>(colrsProductListDataExpression)
                                            .ConfigureAwait(false);

            var productData = JsonConvert.DeserializeObject<ColrsCatalogEntryList>(productDataJson.GetRawText());

            if (productData is null)
            {
                throw new InvalidOperationException("productData is null after deserialising from JsonElement.GetRawText()");
            }

            return productData;
        }
    }
}
