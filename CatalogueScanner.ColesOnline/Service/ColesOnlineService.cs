using CatalogueScanner.ColesOnline.Dto.ColesOnline;
using CatalogueScanner.ColesOnline.Options;
using CatalogueScanner.Core.Http;
using CatalogueScanner.Core.Utility;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization.Metadata;

namespace CatalogueScanner.ColesOnline.Service
{
    public class ColesOnlineService
    {
        private const string ColesBaseUrl = "https://www.coles.com.au/";

        private readonly HttpClient httpClient;
        private readonly ColesOnlineOptions options;
        private readonly CookieContainer cookieContainer = new();

        public ColesOnlineService(HttpClient httpClient, IOptionsSnapshot<ColesOnlineOptions> optionsAccessor)
        {
            #region null checks
            if (optionsAccessor is null)
            {
                throw new ArgumentNullException(nameof(optionsAccessor));
            }
            #endregion

            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            options = optionsAccessor.Value;

            if (httpClient.BaseAddress is null)
            {
                throw new ArgumentException($"{nameof(HttpClient)}.{nameof(HttpClient.BaseAddress)} must be provided", nameof(httpClient));
            }

            cookieContainer.Add(new Cookie
            {
                Name = "fulfillmentStoreId",
                Value = options.FulfillmentStoreId?.ToString(CultureInfo.InvariantCulture),
                Domain = httpClient.BaseAddress!.Host,
            });
        }

        /// <summary>
        /// The time of week when Coles Online changes its specials.
        /// </summary>
        public static TimeOfWeek SpecialsResetTime => new(TimeSpan.Zero, DayOfWeek.Wednesday, "AUS Eastern Standard Time");

        public static Uri ProductUrlTemplate => new($"{ColesBaseUrl}/product/[productId]");

        public async Task<BrowseResponse> GetOnSpecialPageAsync(int page, CancellationToken cancellationToken = default)
        {
            var response = await GetAsync(
                $"on-special.json?page={page}",
                ColesOnlineSerializerContext.Default.BrowseResponse,
                cancellationToken
            ).ConfigureAwait(false);

            if (response is null)
            {
                throw new InvalidOperationException("Browse response is null");
            }

            var searchResults = response.PageProps?.SearchResults;

            if (searchResults is null)
            {
                throw new InvalidOperationException("No search results in browse response");
            }

            return response;
        }

        public async Task<int> GetOnSpecialPageCountAsync(CancellationToken cancellationToken = default)
        {
            var response = await GetOnSpecialPageAsync(1, cancellationToken).ConfigureAwait(false);

            var searchResults = response.PageProps!.SearchResults!;

            return (int)(searchResults.NoOfResults / searchResults.PageSize + 1);
        }

        private async Task<TResponse?> GetAsync<TResponse>(string path, JsonTypeInfo<TResponse> responseTypeInfo, CancellationToken cancellationToken)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(httpClient.BaseAddress!, path));
            request.Headers.Add(HeaderNames.Cookie, cookieContainer.GetCookieHeader(request.RequestUri!));

            using var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

            await response.EnsureSuccessStatusCodeDetailedAsync().ConfigureAwait(false);

            return await response.Content.ReadFromJsonAsync(responseTypeInfo, cancellationToken).ConfigureAwait(false);
        }
    }
}
