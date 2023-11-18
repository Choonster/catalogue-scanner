using System.Text.Json.Serialization;

namespace CatalogueScanner.SaleFinder.Dto.SaleFinder
{
    [JsonSourceGenerationOptions(
        WriteIndented = true,
        PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase
    )]
    [JsonSerializable(typeof(PageWithExtensionData))]
    [JsonSerializable(typeof(Item))]
    [JsonSerializable(typeof(Shape))] // TODO: Remove when https://github.com/dotnet/runtime/pull/62643 is merged/released
    internal sealed partial class SaleFinderInternalSerializerContext : JsonSerializerContext
    {
    }
}
