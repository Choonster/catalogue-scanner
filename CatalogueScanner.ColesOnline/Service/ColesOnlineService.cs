using CatalogueScanner.ColesOnline.Dto.ColesOnline;
using CatalogueScanner.Core.Http;
using CatalogueScanner.Core.Utility;
using System.Net.Http.Json;
using System.Text.Json.Serialization.Metadata;

namespace CatalogueScanner.ColesOnline.Service
{
    public class ColesOnlineService
    {
        private const string ColesBaseUrl = "https://www.coles.com.au/";

        private readonly HttpClient httpClient;

        public ColesOnlineService(HttpClient httpClient)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
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
            var response = await httpClient.GetAsync(new Uri(path, UriKind.Relative), cancellationToken).ConfigureAwait(false);

            await response.EnsureSuccessStatusCodeDetailedAsync().ConfigureAwait(false);

            return await response.Content.ReadFromJsonAsync(responseTypeInfo, cancellationToken).ConfigureAwait(false);
        }
    }
}
