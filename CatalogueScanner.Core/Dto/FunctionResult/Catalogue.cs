using CatalogueScanner.Core.Serialisation;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json.Serialization;

namespace CatalogueScanner.Core.Dto.FunctionResult
{
    public record Catalogue(
        string Store,
        DateTimeOffset StartDate,
        DateTimeOffset EndDate,
        [property: JsonConverter(typeof(CultureInfoConverter))] CultureInfo CurrencyCulture,
        ICollection<CatalogueItem> Items
    );
}
