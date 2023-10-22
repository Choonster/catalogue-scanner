using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace CatalogueScanner.Core.Dto.FunctionResult
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public record CatalogueItem(string? Id, string? Name, string? Sku, Uri? Uri, decimal? Price, long? MultiBuyQuantity, decimal? MultiBuyTotalPrice);
}
