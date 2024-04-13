using CatalogueScanner.Core.Http;
using Microsoft.Identity.Web;
using System.Net.Http.Headers;

namespace CatalogueScanner.ConfigurationUI.Service;

public class BaseApiService
{
    private readonly ILogger<BaseApiService> logger;

    protected HttpClient HttpClient { get; }

    public BaseApiService(HttpClient httpClient, TokenProvider tokenProvider, ILogger<BaseApiService> logger)
    {
        ArgumentNullException.ThrowIfNull(tokenProvider);

        HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        this.logger = logger;
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Constants.Bearer, tokenProvider.AccessToken);
    }

    protected async Task<TResponse?> GetAsync<TResponse>(string path, IDictionary<string, string?>? parameters = null)
    {
        var queryString = parameters is null ? string.Empty : QueryString.Create(parameters).Value;

        var response = await HttpClient.GetAsync(new Uri(path + queryString, UriKind.Relative)).ConfigureAwait(false);

        await response.EnsureSuccessStatusCodeDetailedAsync().ConfigureAwait(false);

        return await response.Content.ReadFromJsonAsync<TResponse>().ConfigureAwait(false);
    }

    protected async Task<TResponse?> PostAsync<TRequest, TResponse>(string path, TRequest? request)
    {
        // Explicitly create the JsonContent and call LoadIntoBufferAsync to prevent the request being sent with Transfer-Encoding: chunked.
        // Microsoft.Azure.Functions.Worker.Extensions.Http version 3.1.0 doesn't support deserialisation of chunked requests into POCO parameters;
        // see https://github.com/Azure/azure-functions-host/issues/7930.

        using var content = JsonContent.Create(request);
        await content.LoadIntoBufferAsync().ConfigureAwait(false);

        var contentString = await content.ReadAsStringAsync().ConfigureAwait(false);

        logger.LogInformation("PostAsync: {Path} - {Request}", path, contentString);

        var response = await HttpClient.PostAsync(new Uri(path, UriKind.Relative), content).ConfigureAwait(false);

        await response.EnsureSuccessStatusCodeDetailedAsync().ConfigureAwait(false);

        if (response.Content.Headers.ContentLength == 0)
        {
            return default;
        }

        return await response.Content.ReadFromJsonAsync<TResponse>().ConfigureAwait(false);
    }
}