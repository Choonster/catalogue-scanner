using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CatalogueScanner.Core.Functions.Api.Management
{
    public static class CleanEntityStorage
    {
        [FunctionName(CoreFunctionNames.CleanEntityStorage)]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Management/CleanEntityStorage")] HttpRequestMessage req,
            [DurableClient] IDurableEntityClient durableEntityClient
        )
        {
            #region null checks
            if (req is null)
            {
                throw new ArgumentNullException(nameof(req));
            }

            if (durableEntityClient is null)
            {
                throw new ArgumentNullException(nameof(durableEntityClient));
            }
            #endregion

            await durableEntityClient.CleanEntityStorageAsync(true, true, CancellationToken.None).ConfigureAwait(false);

            return new OkResult();
        }
    }
}
