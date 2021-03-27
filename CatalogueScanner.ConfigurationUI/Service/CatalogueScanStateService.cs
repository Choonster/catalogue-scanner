using CatalogueScanner.Core.Dto.Api;
using CatalogueScanner.Core.Dto.Api.Request;
using CatalogueScanner.Core.Dto.Api.Result;
using Microsoft.Identity.Web;
using System;
using System.Net.Http;
using System.Net.Http.Formatting;
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
            // Explicitly create the ObjectContent and call LoadIntoBufferAsync to prevent the request being sent with Transfer-Encoding: chunked.
            // Microsoft.Azure.WebJobs.Extensions.Http version 3.0.10 used by azure-functions-core-tools (func.exe) and azure-functions-host doesn't
            // support deserialisation of chunked requests into POCO parameters; see https://github.com/Azure/azure-webjobs-sdk-extensions/issues/702.
            //
            // This was fixed in version 3.0.12 (https://github.com/Azure/azure-webjobs-sdk-extensions/pull/705 ,
            // https://github.com/Azure/azure-webjobs-sdk-extensions/releases/tag/extensions-20210125) but the version used by the host overrides the
            // locally-installed version.
            //
            // Once the host updates to 3.0.12, this shouldn't be necessary.

            using var content = new ObjectContent<TRequest>(request, new JsonMediaTypeFormatter());
            await content.LoadIntoBufferAsync().ConfigureAwait(false);

            var response = await httpClient.PostAsync(new Uri(path, UriKind.Relative), content).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsAsync<TResponse>().ConfigureAwait(false);
        }
    }
}
