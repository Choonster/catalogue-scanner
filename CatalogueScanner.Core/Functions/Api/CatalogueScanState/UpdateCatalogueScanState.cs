using CatalogueScanner.Core.Dto.Api;
using CatalogueScanner.Core.Dto.EntityKey;
using CatalogueScanner.Core.Functions.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System;
using System.Threading.Tasks;

namespace CatalogueScanner.Core.Functions.Api.CatalogueScanState
{
    /// <summary>
    /// Web API function that updates the scan state for a catalogue.
    /// </summary>
    public static class UpdateCatalogueScanState
    {
        [FunctionName(CoreFunctionNames.UpdateCatalogueScanState)]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "CatalogueScanState/Update")] CatalogueScanStateDto dto,
            [DurableClient] IDurableEntityClient durableEntityClient
        )
        {
            #region null checks
            if (dto is null)
            {
                throw new ArgumentNullException(nameof(dto));
            }

            if (durableEntityClient is null)
            {
                throw new ArgumentNullException(nameof(durableEntityClient));
            }
            #endregion

            var entityId = ICatalogueScanState.CreateId(new CatalogueScanStateKey(dto.CatalogueType, dto.Store, dto.CatalogueId));

            await durableEntityClient.SignalEntityAsync<ICatalogueScanState>(
                entityId,
                (scanState) => scanState.UpdateState(dto.ScanState)
            ).ConfigureAwait(false);

            return new OkResult();
        }
    }
}
