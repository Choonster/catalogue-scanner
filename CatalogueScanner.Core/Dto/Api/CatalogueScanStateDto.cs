using CatalogueScanner.Core.Dto.EntityKey;
using CatalogueScanner.Core.Functions.Entity;
using System.Text.Json.Serialization;

namespace CatalogueScanner.Core.Dto.Api;

/// <param name="CatalogueScanStateKey">The unique identifer of the catalogue.</param>
/// <param name="ScanState"> The scan state of the catalogue.</param>
/// <param name="LastModifiedTime">The time of the last change to this scan state record.</param>
public record CatalogueScanStateDto(
    CatalogueScanStateKey CatalogueScanStateKey,
    [property: JsonConverter(typeof(JsonStringEnumMemberConverter))] ScanState ScanState,
    DateTimeOffset LastModifiedTime
);
