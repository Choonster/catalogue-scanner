﻿using CatalogueScanner.Core.Dto.Api;
using CatalogueScanner.Core.Functions.Entity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;
using System.Net;

namespace CatalogueScanner.Core.Functions.Api;

/// <summary>
/// Web API function that updates the scan state for a catalogue.
/// </summary>
public static class UpdateCatalogueScanState
{
    [Function(CoreFunctionNames.UpdateCatalogueScanState)]
    public static async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "CatalogueScanState/Update")] HttpRequestData request,
        [FromBody] CatalogueScanStateDto dto,
        [DurableClient] DurableTaskClient durableTaskClient,
        CancellationToken cancellationToken
    )
    {
        #region null checks
        ArgumentNullException.ThrowIfNull(dto);

        ArgumentNullException.ThrowIfNull(durableTaskClient);
        #endregion

        var entityId = CatalogueScanStateEntity.CreateId(dto.CatalogueScanStateKey);

        await durableTaskClient.Entities.SignalUpdateScanStateAsync(
            entityId,
            dto.ScanState,
            cancellationToken
        ).ConfigureAwait(false);

        return request.CreateResponse(HttpStatusCode.Accepted);
    }
}
