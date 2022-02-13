using CatalogueScanner.Core.Utility;
using CatalogueScanner.WoolworthsOnline.Dto.WoolworthsOnline;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;

namespace CatalogueScanner.WoolworthsOnline.Service
{
    public class WoolworthsOnlineService
    {
        /// <summary>
        /// The maximum value for <see cref="BrowseCategoryRequest.PageSize"/>.
        /// </summary>
        public const int MaxBrowseCategoryDataPageSize = 36;

        private const string WoolworthsBaseUrl = "https://www.woolworths.com.au/";

        private readonly HttpClient httpClient;

        public WoolworthsOnlineService(HttpClient httpClient)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        /// <summary>
        /// The time of week when Coles Online changes its specials.
        /// </summary>
        public static TimeOfWeek SpecialsResetTime => new(TimeSpan.Zero, DayOfWeek.Wednesday, "AUS Eastern Standard Time");

        public static Uri ProductUrlTemplate => new($"{WoolworthsBaseUrl}/shop/productdetails/[stockCode]");


        public async Task<GetPiesCategoriesResponse> GetPiesCategoriesWithSpecialsAsync(CancellationToken cancellationToken = default)
        {
            var response = await GetAsync(
                "PiesCategoriesWithSpecials",
                WoolworthsOnlineSerializerContext.Default.GetPiesCategoriesResponse,
                cancellationToken
            ).ConfigureAwait(false);

            if (response is null)
            {
                throw new InvalidOperationException("PiesCategoriesWithSpecials response is null");
            }

            return response;
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
                throw new InvalidOperationException("Browse Category response is unsuccussful");
            }

            return response;
        }

        public async Task<int> GetCategoryPageCountAsync(string categoryId, int pageSize, CancellationToken cancellationToken = default)
        {
            // Ignore pageSize for this request as we don't actually want any data
            var response = await GetBrowseCategoryDataAsync(new BrowseCategoryRequest
            {
                CategoryId = categoryId,
                PageNumber = 1,
                PageSize = 0,
            }, cancellationToken).ConfigureAwait(false);

            return (int)(response.TotalRecordCount / pageSize + 1);
        }

        private async Task<TResponse?> GetAsync<TResponse>(string path, JsonTypeInfo<TResponse> responseTypeInfo, CancellationToken cancellationToken )
        {
            var response = await httpClient.GetAsync(new Uri(path , UriKind.Relative), cancellationToken).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync(responseTypeInfo, cancellationToken).ConfigureAwait(false);
        }

        private async Task<TResponse?> PostAsync<TRequest, TResponse>(string path, TRequest request, JsonTypeInfo<TRequest> requestTypeInfo, JsonTypeInfo<TResponse> responseTypeInfo, CancellationToken cancellationToken)
        {
            var response = await httpClient.PostAsJsonAsync(new Uri(path, UriKind.Relative), request, requestTypeInfo, cancellationToken).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync(responseTypeInfo, cancellationToken).ConfigureAwait(false);
        }
    }
}
