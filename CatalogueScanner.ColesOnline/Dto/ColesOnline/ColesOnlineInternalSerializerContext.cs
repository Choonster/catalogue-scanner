using System.Text.Json.Serialization;

namespace CatalogueScanner.ColesOnline.Dto.ColesOnline
{
    [JsonSourceGenerationOptions(
        WriteIndented = true,
        PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase
    )]
    [JsonSerializable(typeof(Product))]
    [JsonSerializable(typeof(SingleTile))]
    internal sealed partial class ColesOnlineInternalSerializerContext : JsonSerializerContext
    {
    }
}
