using Microsoft.AspNetCore.Http;
using Microsoft.Identity.Web;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace CatalogueScanner.ConfigurationUI.Service
{
    public class BaseApiService
    {
        protected HttpClient HttpClient { get; }

        public BaseApiService(HttpClient httpClient, TokenProvider tokenProvider)
        {
            if (tokenProvider is null)
            {
                throw new ArgumentNullException(nameof(tokenProvider));
            }

            HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Constants.Bearer, tokenProvider.AccessToken);
        }

        protected async Task<TResponse> GetAsync<TResponse>(string path, IDictionary<string, string?>? parameters = null)
        {
            var queryString = parameters is null ? string.Empty : QueryString.Create(parameters).Value;

            var response = await HttpClient.GetAsync(new Uri(path + queryString, UriKind.Relative)).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsAsync<TResponse>().ConfigureAwait(false);
        }

        protected async Task<TResponse> PostAsync<TRequest, TResponse>(string path, TRequest? request)
        {
            var response = await HttpClient.PostAsJsonAsync(new Uri(path, UriKind.Relative), request).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsAsync<TResponse>().ConfigureAwait(false);
        }
    }
}