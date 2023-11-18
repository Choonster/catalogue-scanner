using CatalogueScanner.Core.Dto.FunctionResult;
using Microsoft.DurableTask.Entities;

namespace CatalogueScanner.Core.Dto.FunctionInput;

public record FilterCatalogueAndSendDigestEmailInput(Catalogue Catalogue, EntityInstanceId ScanStateId);
