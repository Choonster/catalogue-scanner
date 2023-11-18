using CatalogueScanner.Core.Dto.Api;
using CatalogueScanner.Core.Dto.Api.Request;
using CatalogueScanner.Core.Dto.Api.Result;
using CatalogueScanner.Core.Dto.EntityKey;
using CatalogueScanner.Core.Functions.Entity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;
using Microsoft.DurableTask.Client.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CatalogueScanner.Core.Functions.Api
{
    /// <summary>
    /// Web API function that lists catalogue scan states.
    /// </summary>
    public static class ListCatalogueScanStates
    {
        [Function(CoreFunctionNames.ListCatalogueScanStates)]
        public static async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "CatalogueScanState/List")] HttpRequestData request,
            [FromBody] ListEntityRequest listEntityRequest,
            [DurableClient] DurableTaskClient durableTaskClient,
            CancellationToken cancellationToken
        )
        {
            #region null checks
            ArgumentNullException.ThrowIfNull(request);

            ArgumentNullException.ThrowIfNull(listEntityRequest);

            ArgumentNullException.ThrowIfNull(durableTaskClient);
            #endregion

            var query = new EntityQuery
            {
                InstanceIdStartsWith = CoreFunctionNames.CatalogueScanState,
                LastModifiedFrom = listEntityRequest.LastModifiedFrom,
                LastModifiedTo = listEntityRequest.LastModifiedTo,
                PageSize = listEntityRequest.Page.PageSize,
                ContinuationToken = listEntityRequest.Page.ContinuationToken,
                IncludeState = true,
            };

            var result = durableTaskClient.Entities.GetAllEntitiesAsync(query);

            var page = await result.AsPages().FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

            if (page is null)
            {
                return await Response(Enumerable.Empty<CatalogueScanStateDto>(), null).ConfigureAwait(false);
            }

            var entities = page.Values.Select(metatada =>
            {
                var key = CatalogueScanStateKey.FromString(metatada.Id.Key);
                var state = metatada.State.ReadAs<ScanState>();

                return new CatalogueScanStateDto(key, state, metatada.LastModifiedTime);
            });

            return await Response(entities, page.ContinuationToken).ConfigureAwait(false);

            async Task<HttpResponseData> Response(IEnumerable<CatalogueScanStateDto> entities, string? continuationToken)
            {
                var result = new ListEntityResult<CatalogueScanStateDto>(entities, listEntityRequest.Page with { ContinuationToken = continuationToken });

                var response = request.CreateResponse();

                await response.WriteAsJsonAsync(result, cancellationToken).ConfigureAwait(false);

                return response;
            }
        }
    }
}
