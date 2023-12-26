using System.Text.Json.Serialization;

namespace CatalogueScanner.SaleFinder.Dto.SaleFinder;

[JsonSourceGenerationOptions(
  WriteIndented = true,
  PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase
)]
[JsonSerializable(typeof(SaleFinderCatalogue))]
[JsonSerializable(typeof(CatalogueViewResponse))]
public partial class SaleFinderSerializerContext : JsonSerializerContext
{
}
