using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CatalogueScanner.Dto.SaleFinder;

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

        public Task<Catalogue> GetCatalogueAsync(int saleId)
        {
            return GetAsync<Catalogue>($"/catalogue/svgData/{saleId}/?format=json");
        }

        private async Task<T> GetAsync<T>(string path)
        {
            var response = await httpClient.GetAsync(new Uri(path, UriKind.Relative)).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadSaleFinderResponseAsAync<T>().ConfigureAwait(false);
        }
    }
}
