using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;
using System;
using System.Threading;

namespace CatalogueScanner.Core.Functions.Api.Management
{
    public static class GetCheckStatusEndpoints
    {
        [Function(CoreFunctionNames.GetCheckStatusEndpoints)]
        public static HttpResponseData Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Management/CheckStatusEndpoints/{instanceId?}")] HttpRequestData req,
            [DurableClient] DurableTaskClient durableTaskClient,
            string instanceId,
            CancellationToken cancellationToken
        )
        {
            #region null checks
            if (req is null)
            {
                throw new ArgumentNullException(nameof(req));
            }

            if (durableTaskClient is null)
            {
                throw new ArgumentNullException(nameof(durableTaskClient));
            }
            #endregion

            return durableTaskClient.CreateCheckStatusResponse(req, instanceId, cancellationToken);
        }
    }
}
