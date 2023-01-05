using CatalogueScanner.Core.Http;
using CatalogueScanner.SaleFinder.Dto.SaleFinder;
using System;
using System.Net.Http;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;

namespace CatalogueScanner.SaleFinder.Service
{
    public class SaleFinderService
    {
        /// <summary>
        /// A dummy value to pass as the <c>callback</c> parameter of SaleFinder requests.
        /// </summary>
        private const string CALLBACK_NAME = "____________________";

        private readonly HttpClient httpClient;

        public SaleFinderService(HttpClient httpClient)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<CatalogueViewResponse?> GetCatalogueViewDataAsync(int storeId, int locationId, CancellationToken cancellationToken = default) =>
            await GetAsync($"/catalogues/view/{storeId}/?format=json&locationId={locationId}", SaleFinderSerializerContext.Default.CatalogueViewResponse, cancellationToken: cancellationToken).ConfigureAwait(false);

        public async Task<SaleFinderCatalogue?> GetCatalogueAsync(int saleId, CancellationToken cancellationToken = default) =>
            await GetAsync($"/catalogue/svgData/{saleId}/?format=json", SaleFinderSerializerContext.Default.SaleFinderCatalogue, CALLBACK_NAME, cancellationToken).ConfigureAwait(false);

        public async Task<string?> GetItemTooltipAsync(long itemId, CancellationToken cancellationToken = default) =>
            await GetAsync($"/item/tooltip/{itemId}?", SaleFinderSerializerContext.Default.String, cancellationToken: cancellationToken).ConfigureAwait(false);

        private async Task<T?> GetAsync<T>(string path, JsonTypeInfo<T> jsonTypeInfo, string? callbackName = null, CancellationToken cancellationToken = default)
        {
            var response = await httpClient.GetAsync(new Uri(path + $"&callback={callbackName}", UriKind.Relative), cancellationToken).ConfigureAwait(false);

            await response.EnsureSuccessStatusCodeDetailedAsync().ConfigureAwait(false);

            return await response.Content.ReadSaleFinderResponseAsAync(jsonTypeInfo, callbackName, cancellationToken).ConfigureAwait(false);
        }
    }
}
