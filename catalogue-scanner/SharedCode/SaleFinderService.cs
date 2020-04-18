using CatalogueScanner.Dto.SaleFinder;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace CatalogueScanner
{
    public class SaleFinderService
    {
        private readonly HttpClient httpClient;

        public SaleFinderService(HttpClient httpClient)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

            this.httpClient.BaseAddress = new Uri("https://embed.salefinder.com.au/");
        }

        public Task<CatalogueViewResponse> GetCatalogueViewDataAsync(int storeId, int locationId)
        {
            return GetAsync<CatalogueViewResponse>($"/catalogues/view/{storeId}/?format=json&locationId={locationId}");
        }

        public Task<SaleFinderCatalogue> GetCatalogueAsync(int saleId)
        {
            return GetAsync<SaleFinderCatalogue>($"/catalogue/svgData/{saleId}/?format=json");
        }

        private async Task<T> GetAsync<T>(string path)
        {
            var response = await httpClient.GetAsync(new Uri(path, UriKind.Relative)).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadSaleFinderResponseAsAync<T>().ConfigureAwait(false);
        }
    }
}
