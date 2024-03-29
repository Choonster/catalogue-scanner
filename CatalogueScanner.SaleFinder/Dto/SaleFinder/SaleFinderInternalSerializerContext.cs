﻿using System.Text.Json.Serialization;

namespace CatalogueScanner.SaleFinder.Dto.SaleFinder;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase
)]
[JsonSerializable(typeof(PageWithExtensionData))]
[JsonSerializable(typeof(Item))]
internal sealed partial class SaleFinderInternalSerializerContext : JsonSerializerContext
{
}
