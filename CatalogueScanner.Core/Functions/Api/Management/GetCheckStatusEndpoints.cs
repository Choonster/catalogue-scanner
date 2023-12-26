using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;
using System;
using System.Threading;

namespace CatalogueScanner.Core.Functions.Api.Management;

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
        ArgumentNullException.ThrowIfNull(req);

        ArgumentNullException.ThrowIfNull(durableTaskClient);

        ArgumentNullException.ThrowIfNull(instanceId);
        #endregion

        return durableTaskClient.CreateCheckStatusResponse(req, instanceId, cancellationToken);
    }
}
