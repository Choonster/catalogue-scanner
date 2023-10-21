using System;
using System.Text.Json.Serialization;

namespace CatalogueScanner.SaleFinder.Dto.SaleFinder
{
    [JsonSourceGenerationOptions(
      WriteIndented = true,
      PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase
    )]
    [JsonSerializable(typeof(SaleFinderCatalogue))]
    [JsonSerializable(typeof(CatalogueViewResponse))]
    [JsonSerializable(typeof(DateTimeOffset))] // TODO: Remove when https://github.com/dotnet/runtime/pull/62643 is merged/released
    [JsonSerializable(typeof(long))]
    public partial class SaleFinderSerializerContext : JsonSerializerContext
    {
    }
}
