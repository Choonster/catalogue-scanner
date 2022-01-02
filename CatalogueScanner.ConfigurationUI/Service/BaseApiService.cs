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

            var response = await HttpClient.PostAsync(new Uri(path, UriKind.Relative), content).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsAsync<TResponse>().ConfigureAwait(false);
        }
    }
}