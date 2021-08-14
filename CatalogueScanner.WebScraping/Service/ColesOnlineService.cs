using CatalogueScanner.Core.Utility;
using CatalogueScanner.WebScraping.Common.Dto.ColesOnline;
using CatalogueScanner.WebScraping.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace CatalogueScanner.WebScraping.Service
{
    public class ColesOnlineService
    {
        private readonly HttpClient httpClient;
        private readonly WebScrapingApiOptions webScrapingApiOptions;
        private readonly ITokenAcquisition tokenAcquisition;
        private readonly string tokenScope;

        public ColesOnlineService(HttpClient httpClient, IOptionsSnapshot<WebScrapingApiOptions> webScrapingApiOptions, ITokenAcquisition tokenAcquisition)
        {
            #region null checks
            if (webScrapingApiOptions is null)
            {
                throw new ArgumentNullException(nameof(webScrapingApiOptions));
            }
            #endregion

            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            this.tokenAcquisition = tokenAcquisition ?? throw new ArgumentNullException(nameof(tokenAcquisition));
            this.webScrapingApiOptions = webScrapingApiOptions.Value;

            tokenScope = new Uri(this.webScrapingApiOptions.BaseAddress!, ".default").AbsoluteUri;
        }

        /// <summary>
        /// The time of week when Coles Online changes its specials.
        /// </summary>
        public static TimeOfWeek SpecialsResetTime => new TimeOfWeek(TimeSpan.Zero, DayOfWeek.Wednesday, "AUS Eastern Standard Time");

        /// <summary>
        /// Gets the current specials from Coles Online.
        /// </summary>
        /// <returns>The specials</returns>
        public async Task<ColesOnlineSpecialsResult> GetColesOnlineSpecials()
        {
            return await GetAsync<ColesOnlineSpecialsResult>("specials").ConfigureAwait(false);
        }

        private async Task<T> GetAsync<T>(string path)
        {
            var token = await tokenAcquisition.GetAccessTokenForAppAsync(tokenScope, authenticationScheme: Constants.Bearer).ConfigureAwait(false);

            using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(path, UriKind.Relative));
            request.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, token);

            var response = await httpClient.SendAsync(request).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsAsync<T>().ConfigureAwait(false);

            if (result is null)
            {
                throw new JsonSerializationException("result is null");
            }

            return result;
        }
    }
}
