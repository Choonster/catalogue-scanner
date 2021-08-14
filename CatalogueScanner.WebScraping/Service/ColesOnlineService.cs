using CatalogueScanner.Core.Utility;
using CatalogueScanner.WebScraping.Common.Dto.ColesOnline;
using Newtonsoft.Json;
using System;
using System.Net.Http;
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
            var response = await httpClient.GetAsync(path).ConfigureAwait(false);

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
