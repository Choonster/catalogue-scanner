using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace CatalogueScanner.ConfigurationUI.Service
{
    public class ManagementService(HttpClient httpClient, TokenProvider tokenProvider) : BaseApiService(httpClient, tokenProvider)
    {
        public async Task<IDictionary<string, string>?> GetCheckStatusEndpointsAsync(string? instanceId) =>
            await GetAsync<IDictionary<string, string>>($"CheckStatusEndpoints/{instanceId}").ConfigureAwait(false);

        public async Task CleanEntityStorageAsync() =>
            await PostAsync<object, object>("CleanEntityStorage", null).ConfigureAwait(false);
    }
}
