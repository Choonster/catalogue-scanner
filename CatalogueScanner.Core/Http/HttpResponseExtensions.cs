using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace CatalogueScanner.Core.Http
{
    public static class HttpResponseMessageExtensions
    {
        public static async Task<HttpResponseMessage> EnsureSuccessStatusCodeDetailedAsync(this HttpResponseMessage response)
        {
            #region null checks
            if (response is null)
            {
                throw new ArgumentNullException(nameof(response));
            }
            #endregion

            if (!response.IsSuccessStatusCode)
            {
                var message = $"Response status code does not indicate success: {(int)response.StatusCode} ({response.ReasonPhrase}).";

                try
                {
                    var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    throw new HttpDetailedRequestException(message, null, response.StatusCode, responseContent);
                }
                catch (Exception ex)
                {
                    throw new HttpDetailedRequestException(message, ex, response.StatusCode, null);
                }
            }

            return response;
        }
    }
}
