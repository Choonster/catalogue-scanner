using CatalogueScanner.SaleFinder.Dto.SaleFinder;
using System;
using System.Net.Http;
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

        public Task<CatalogueViewResponse> GetCatalogueViewDataAsync(int storeId, int locationId)
        {
            return GetAsync<CatalogueViewResponse>($"/catalogues/view/{storeId}/?format=json&locationId={locationId}");
        }

        public Task<SaleFinderCatalogue> GetCatalogueAsync(int saleId)
        {
            return GetAsync<SaleFinderCatalogue>($"/catalogue/svgData/{saleId}/?format=json", CALLBACK_NAME);
        }

        private async Task<T> GetAsync<T>(string path, string? callbackName = null)
        {
            var response = await httpClient.GetAsync(new Uri(path + $"&callback={callbackName}", UriKind.Relative)).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadSaleFinderResponseAsAync<T>(callbackName).ConfigureAwait(false);
        }
    }
}
