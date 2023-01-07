using System.Text.Json.Serialization;

namespace CatalogueScanner.ColesOnline.Dto.ColesOnline
{
    [JsonSourceGenerationOptions(
        WriteIndented = true,
        PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase
    )]
    [JsonSerializable(typeof(BrowseResponse))]
    public partial class ColesOnlineSerializerContext : JsonSerializerContext
    {
    }
}
