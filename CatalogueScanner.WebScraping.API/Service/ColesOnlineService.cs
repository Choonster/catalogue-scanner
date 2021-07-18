using CatalogueScanner.WebScraping.API.Options;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace CatalogueScanner.WebScraping.API.Service
{
    public class ColesOnlineService
    {
        private const string ColesBaseUrl = "https://shop.coles.com.au/a";

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

        public async Task<dynamic> GetSpecialsAsync()
        {
            using var playwright = await Playwright.CreateAsync().ConfigureAwait(false);

            await using var browser = await playwright.Chromium.LaunchAsync().ConfigureAwait(false);

            var page = await browser.NewPageAsync().ConfigureAwait(false);

            var response = await page.GotoAsync(
                $"{ColesBaseUrl}/{options.StoreId}/specials/browse",
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
                throw new HttpRequestException($"Coles Online request failed with status {response.Status} ({response.StatusText})");
            }

            await page.WaitForLoadStateAsync(LoadState.Load).ConfigureAwait(false);

            await page.WaitForLoadStateAsync(LoadState.NetworkIdle).ConfigureAwait(false);

            dynamic productData = await page.EvaluateAsync<dynamic>(@"angular.element(document.querySelector(""[data-colrs-product-list]"")).data().$colrsProductListController.widget.data")
                                            .ConfigureAwait(false);

            return productData;
        }
    }
}
