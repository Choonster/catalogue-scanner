using CatalogueScanner.WebScraping.Common.Dto.ColesOnline;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace CatalogueScanner.WebScraping.Service
{
    public class ColesOnlineService
    {
        private readonly HttpClient httpClient;

        public ColesOnlineService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<ColesOnlineSpecialsResult> GetColesOnlineSpecials()
        {
            return await GetAsync<ColesOnlineSpecialsResult>("specials").ConfigureAwait(false);
        }

        private async Task<T> GetAsync<T>(string path)
        {
            var response = await httpClient.GetAsync(path).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsAsync<T>().ConfigureAwait(false);

            if (result is null)
            {
                throw new JsonException("result is null");
            }

            return result;
        }
    }
}
