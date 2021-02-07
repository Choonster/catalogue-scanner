using CatalogueScanner.Core.Dto.Api;
using CatalogueScanner.Core.Dto.Api.Request;
using CatalogueScanner.Core.Dto.Api.Result;
using Microsoft.Identity.Web;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace CatalogueScanner.ConfigurationUI.Service
{
    public class CatalogueScanStateService
    {
        private readonly HttpClient httpClient;

        public CatalogueScanStateService(HttpClient httpClient, TokenProvider tokenProvider)
        {
            if (tokenProvider is null)
            {
                throw new ArgumentNullException(nameof(tokenProvider));
            }

            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Constants.Bearer, tokenProvider.AccessToken);
        }

        public async Task<ListEntityResult<CatalogueScanStateDto>> ListCatalogueScanStatesAsync(ListEntityRequest listEntityRequest)
        {
            return await PostAsync<ListEntityRequest, ListEntityResult<CatalogueScanStateDto>>("List", listEntityRequest).ConfigureAwait(false);
        }

        public async Task UpdateCatalogueScanStateAsync(CatalogueScanStateDto dto)
        {
            await PostAsync<CatalogueScanStateDto, object>("Update", dto).ConfigureAwait(false);
        }

        private async Task<TResponse> PostAsync<TRequest, TResponse>(string path, TRequest request)
        {
            var response = await httpClient.PostAsJsonAsync(new Uri(path, UriKind.Relative), request).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsAsync<TResponse>().ConfigureAwait(false);
        }
    }
}
