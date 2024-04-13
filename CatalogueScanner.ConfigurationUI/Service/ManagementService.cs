namespace CatalogueScanner.ConfigurationUI.Service;

public class ManagementService(HttpClient httpClient, TokenProvider tokenProvider) : BaseApiService(httpClient, tokenProvider)
{
    public async Task<IDictionary<string, string>?> GetCheckStatusEndpointsAsync(string? instanceId, CancellationToken cancellationToken = default) =>
        await GetAsync<IDictionary<string, string>>($"CheckStatusEndpoints/{instanceId}", cancellationToken: cancellationToken).ConfigureAwait(false);

    public async Task CleanEntityStorageAsync(CancellationToken cancellationToken = default) =>
        await PostAsync<object, object>("CleanEntityStorage", null, cancellationToken).ConfigureAwait(false);
}
