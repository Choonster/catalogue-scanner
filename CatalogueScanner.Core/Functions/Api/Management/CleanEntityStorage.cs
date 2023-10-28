using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;
using Microsoft.DurableTask.Client.Entities;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace CatalogueScanner.Core.Functions.Api.Management
{
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
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (durableTaskClient is null)
            {
                throw new ArgumentNullException(nameof(durableTaskClient));
            }
            #endregion

            await durableTaskClient.Entities.CleanEntityStorageAsync(
                new CleanEntityStorageRequest { RemoveEmptyEntities = true, ReleaseOrphanedLocks = true },
                cancellation: cancellationToken
            ).ConfigureAwait(false);

            return request.CreateResponse(HttpStatusCode.OK);
        }
    }
}
