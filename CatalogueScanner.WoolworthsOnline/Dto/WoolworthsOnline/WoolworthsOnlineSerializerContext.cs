using System.Text.Json.Serialization;

namespace CatalogueScanner.WoolworthsOnline.Dto.WoolworthsOnline
{
    [JsonSourceGenerationOptions(
       WriteIndented = true
    )]
    [JsonSerializable(typeof(BrowseCategoryRequest))]
    [JsonSerializable(typeof(BrowseCategoryResponse))]
    [JsonSerializable(typeof(GetPiesCategoriesResponse))]
    [JsonSerializable(typeof(Promotion))] // TODO: Remove when https://github.com/dotnet/runtime/pull/62643 is merged/released
    public partial class WoolworthsOnlineSerializerContext : JsonSerializerContext
    {
    }
}
