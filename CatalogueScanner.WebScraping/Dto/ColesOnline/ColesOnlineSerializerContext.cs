using System.Text.Json.Serialization;

namespace CatalogueScanner.WebScraping.Dto.ColesOnline
{
    [JsonSourceGenerationOptions(
        WriteIndented = true,
        PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase
    )]
    [JsonSerializable(typeof(ColrsCatalogEntryList))]
    [JsonSerializable(typeof(PromoType))] // TODO: Remove when https://github.com/dotnet/runtime/pull/62643 is merged/released
    public partial class ColesOnlineSerializerContext : JsonSerializerContext
    {
    }
}
