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

            string responseContent;
            try
            {
                responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new HttpDetailedRequestException(message, ex, response.StatusCode, null);
            }

            throw new HttpDetailedRequestException(message, null, response.StatusCode, responseContent);
        }

        return response;
    }
}
