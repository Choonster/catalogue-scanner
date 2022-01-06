using System.Text.Json.Serialization;

namespace CatalogueScanner.SaleFinder.Dto.SaleFinder
{
    [JsonSourceGenerationOptions(
        WriteIndented = true,
        PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase
    )]
    [JsonSerializable(typeof(PageWithExtensionData))]
    [JsonSerializable(typeof(Shape))] // TODO: Remove when https://github.com/dotnet/runtime/pull/62643 is merged/released
    internal partial class SaleFinderInternalSerializerContext : JsonSerializerContext
    {
    }
}
