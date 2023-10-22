using CatalogueScanner.Core.Dto.FunctionResult;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace CatalogueScanner.Core.Dto.FunctionInput
{
    public record FilterCatalogueAndSendDigestEmailInput(Catalogue Catalogue, EntityId ScanStateId);
}
