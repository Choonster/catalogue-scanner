using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;
using Microsoft.DurableTask.Client.Entities;
using System.Net;

namespace CatalogueScanner.Core.Functions.Api.Management;

public static class CleanEntityStorage
{
    [Function(CoreFunctionNames.CleanEntityStorage)]
    public static async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Management/CleanEntityStorage")] HttpRequestData request,
        [DurableClient] DurableTaskClient durableTaskClient,
        CancellationToken cancellationToken
    )
    {
        #region null checks
        ArgumentNullException.ThrowIfNull(request);

        ArgumentNullException.ThrowIfNull(durableTaskClient);
        #endregion

        await durableTaskClient.Entities.CleanEntityStorageAsync(
            new CleanEntityStorageRequest { RemoveEmptyEntities = true, ReleaseOrphanedLocks = true },
            cancellation: cancellationToken
        ).ConfigureAwait(false);

        return request.CreateResponse(HttpStatusCode.OK);
    }
}
