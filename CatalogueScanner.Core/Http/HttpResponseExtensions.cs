namespace CatalogueScanner.Core.Http;

public static class HttpResponseMessageExtensions
{
    public static async Task<HttpResponseMessage> EnsureSuccessStatusCodeDetailedAsync(this HttpResponseMessage response)
    {
        #region null checks
        ArgumentNullException.ThrowIfNull(response);
        #endregion

        if (!response.IsSuccessStatusCode)
        {
            var message = $"Response status code does not indicate success: {(int)response.StatusCode} ({response.ReasonPhrase}).";

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
                throw new HttpDetailedRequestException(message, ex, response.StatusCode, null, null);
            }

            string responseContent;
            try
            {
                responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new HttpDetailedRequestException(message, ex, response.StatusCode, null, null);
            }

            throw new HttpDetailedRequestException(message, null, response.StatusCode, requestContent, responseContent);
        }

        return response;
    }
}
