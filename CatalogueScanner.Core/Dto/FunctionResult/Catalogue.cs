using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace CatalogueScanner.Core.Dto.FunctionResult
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public record Catalogue(string Store, DateTimeOffset StartDate, DateTimeOffset EndDate, CultureInfo CurrencyCulture, ICollection<CatalogueItem> Items);
}
