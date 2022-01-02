using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System;
using System.Net.Http;

namespace CatalogueScanner.Core.Functions.Api.Management
{
    public static class GetCheckStatusEndpoints
    {
        [FunctionName(CoreFunctionNames.GetCheckStatusEndpoints)]
        public static HttpResponseMessage Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Management/CheckStatusEndpoints/{instanceId?}")] HttpRequestMessage req,
            [DurableClient] IDurableClient durableClient,
            string? instanceId = null
        )
        {
            #region null checks
            if (req is null)
            {
                throw new ArgumentNullException(nameof(req));
            }

            if (durableClient is null)
            {
                throw new ArgumentNullException(nameof(durableClient));
            }
            #endregion

            return durableClient.CreateCheckStatusResponse(req, instanceId);
        }
    }
}
