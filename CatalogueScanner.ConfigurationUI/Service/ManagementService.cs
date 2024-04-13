namespace CatalogueScanner.ConfigurationUI.Service;

public class ManagementService(HttpClient httpClient, TokenProvider tokenProvider, ILogger<ManagementService> logger) : BaseApiService(httpClient, tokenProvider, logger)
{
    public async Task<IDictionary<string, string>?> GetCheckStatusEndpointsAsync(string? instanceId, CancellationToken cancellationToken = default) =>
        await GetAsync<IDictionary<string, string>>($"CheckStatusEndpoints/{instanceId}", cancellationToken: cancellationToken).ConfigureAwait(false);

    public async Task CleanEntityStorageAsync(CancellationToken cancellationToken = default) =>
        await PostAsync<object, object>("CleanEntityStorage", null, cancellationToken).ConfigureAwait(false);
}
