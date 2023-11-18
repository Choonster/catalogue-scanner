using System.Text.Json.Serialization;

namespace CatalogueScanner.WoolworthsOnline.Dto.WoolworthsOnline;

[JsonSourceGenerationOptions(
   WriteIndented = true
)]
[JsonSerializable(typeof(BrowseCategoryRequest))]
[JsonSerializable(typeof(BrowseCategoryResponse))]
[JsonSerializable(typeof(GetPiesCategoriesResponse))]
public partial class WoolworthsOnlineSerializerContext : JsonSerializerContext
{
}
