using CatalogueScanner.Core.Dto.Api;
using CatalogueScanner.Core.Dto.Api.Request;
using CatalogueScanner.Core.Dto.Api.Result;
using CatalogueScanner.Core.Functions.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CatalogueScanner.Core.Functions.Api.CatalogueScanState
{
    /// <summary>
    /// Web API function that lists catalogue scan states.
    /// </summary>
    public static class ListCatalogueScanStates
    {
        [FunctionName(CoreFunctionNames.ListCatalogueScanStates)]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "CatalogueScanState/List")] ListEntityRequest listEntityRequest,
            [DurableClient] IDurableEntityClient durableEntityClient
        )
        {
            #region null checks
            if (listEntityRequest is null)
            {
                throw new ArgumentNullException(nameof(listEntityRequest));
            }

            if (durableEntityClient is null)
            {
                throw new ArgumentNullException(nameof(durableEntityClient));
            }
            #endregion

            var query = new EntityQuery
            {
                EntityName = ICatalogueScanState.EntityName,
                LastOperationFrom = listEntityRequest.LastOperationFrom.GetValueOrDefault(),
                LastOperationTo = listEntityRequest.LastOperationTo.GetValueOrDefault(),
                PageSize = listEntityRequest.Page.PageSize,
                ContinuationToken = listEntityRequest.Page.ContinuationToken,
                FetchState = true,
            };

            var result = await durableEntityClient.ListEntitiesAsync(query, default).ConfigureAwait(false);

            var entities = result.Entities.Select(status =>
            {
                var dto = status.State.ToObject<CatalogueScanStateDto>();

                if (dto is null)
                {
                    throw new InvalidOperationException($"Entity with ID {status.EntityId} has no State");
                }

                dto.LastOperationTime = status.LastOperationTime;

                return dto;
            });

            return new ObjectResult(new ListEntityResult<CatalogueScanStateDto>
            {
                Entities = entities,
                Page = new PageInfo
                {
                    ContinuationToken = result.ContinuationToken,
                    PageSize = query.PageSize
                }
            });
        }
    }
}
