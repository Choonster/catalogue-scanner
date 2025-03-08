using System.Text.Json.Serialization;

namespace CatalogueScanner.Core.Host.ApplicationInsights;

[JsonSourceGenerationOptions(
  WriteIndented = true
)]
[JsonSerializable(typeof(IReadOnlyDictionary<string, IEnumerable<string>>))]
public partial class ApplicationInsightsSerializerContext : JsonSerializerContext
{
}
