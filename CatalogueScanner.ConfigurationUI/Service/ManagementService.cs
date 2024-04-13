namespace CatalogueScanner.ConfigurationUI.Service;

public class ManagementService(HttpClient httpClient, TokenProvider tokenProvider, ILogger<ManagementService> logger) : BaseApiService(httpClient, tokenProvider, logger)
{
    public async Task<IDictionary<string, string>?> GetCheckStatusEndpointsAsync(string? instanceId) =>
        await GetAsync<IDictionary<string, string>>($"CheckStatusEndpoints/{instanceId}").ConfigureAwait(false);

    public async Task CleanEntityStorageAsync() =>
        await PostAsync<object, object>("CleanEntityStorage", null).ConfigureAwait(false);
}
