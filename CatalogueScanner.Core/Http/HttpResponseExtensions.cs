using System.Collections.Immutable;
using System.Net.Http.Headers;

namespace CatalogueScanner.Core.Http;

public static class HttpResponseMessageExtensions
{
    private static readonly ImmutableHashSet<string> IgnoredHeaders = ["Authorization"];

    public static async Task<HttpResponseMessage> EnsureSuccessStatusCodeDetailedAsync(this HttpResponseMessage response)
    {
        #region null checks
        ArgumentNullException.ThrowIfNull(response);
        #endregion

        if (!response.IsSuccessStatusCode)
        {
            var message = $"Response status code does not indicate success: {(int)response.StatusCode} ({response.ReasonPhrase}).";

            var requestHeaders = FilterHeaders(response.RequestMessage?.Headers);
            var responseHeaders = FilterHeaders(response.Headers);

            string? requestContent;
            try
            {
                if (response.RequestMessage?.Content is null)
                {
                    requestContent = null;
                }
                else
                {
                    requestContent = await response.RequestMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                throw new HttpDetailedRequestException(message, ex, response.StatusCode, null, null, requestHeaders, responseHeaders);
            }

            string responseContent;
            try
            {
                responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new HttpDetailedRequestException(message, ex, response.StatusCode, null, null, requestHeaders, responseHeaders);
            }

            throw new HttpDetailedRequestException(message, null, response.StatusCode, requestContent, responseContent, requestHeaders, responseHeaders);
        }

        return response;
    }

    private static ImmutableDictionary<string, IEnumerable<string>>? FilterHeaders(HttpHeaders? headers)
    {
        if (headers is null)
        {
            return null;
        }

        var dictionary = headers.ToDictionary(h => h.Key, h => h.Value);
        
        foreach (var header in dictionary.Where(header => IgnoredHeaders.Contains(header.Key)))
        {
            dictionary[header.Key] = ["<REDACTED>"];
        }

        return dictionary.ToImmutableDictionary();
    }
}
