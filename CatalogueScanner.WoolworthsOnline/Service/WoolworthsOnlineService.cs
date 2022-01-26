using CatalogueScanner.WoolworthsOnline.Dto.WoolworthsOnline;
using System.Net.Http.Json;
using System.Text.Json.Serialization.Metadata;

namespace CatalogueScanner.WoolworthsOnline.Service
{
    public class WoolworthsOnlineService
    {
        private readonly HttpClient httpClient;

        public WoolworthsOnlineService(HttpClient httpClient)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<BrowseCategoryResponse> GetBrowseCategoryDataAsync(BrowseCategoryRequest request, CancellationToken cancellationToken = default)
        {
            #region null checks
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }
            #endregion

            var response = await PostAsync(
                "browse/category",
                request,
                WoolworthsOnlineSerializerContext.Default.BrowseCategoryRequest,
                WoolworthsOnlineSerializerContext.Default.BrowseCategoryResponse,
                cancellationToken
            ).ConfigureAwait(false);

            if (response is null)
            {
                throw new InvalidOperationException("Browse Category response is null");
            }

            if (!response.Success)
            {
                throw new InvalidOperationException("Browse Categoru response is unsuccussful");
            }

            return response;
        }

        private async Task<TResponse?> PostAsync<TRequest, TResponse>(string path, TRequest request, JsonTypeInfo<TRequest> requestTypeInfo, JsonTypeInfo<TResponse> responseTypeInfo, CancellationToken cancellationToken)
        {
            var response = await httpClient.PostAsJsonAsync(new Uri(path, UriKind.Relative), request, requestTypeInfo, cancellationToken).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync(responseTypeInfo, cancellationToken).ConfigureAwait(false);
        }
    }
}
