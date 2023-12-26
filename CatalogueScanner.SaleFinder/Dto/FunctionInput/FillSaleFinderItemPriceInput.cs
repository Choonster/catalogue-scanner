using CatalogueScanner.Core.Dto.FunctionResult;
using CatalogueScanner.Core.Serialisation;
using System.Globalization;
using System.Text.Json.Serialization;

namespace CatalogueScanner.SaleFinder.Dto.FunctionInput;

public record FillSaleFinderItemPriceInput(
    [property: JsonConverter(typeof(CultureInfoConverter))] CultureInfo CurrencyCulture,
    CatalogueItem Item
);
