using CatalogueScanner.Core.Serialisation;
using System.Globalization;
using System.Text.Json.Serialization;

namespace CatalogueScanner.SaleFinder.Dto.FunctionResult;

/// <summary>
/// Represents a SaleFinder catalogue to be scanned.
/// </summary>
/// <param name="SaleId"> The ID of the SaleFinder catalogue. </param>
/// <param name="BaseUri"> The base URI of the SaleFinder catalogue page. </param>
/// <param name="Store"> The store that the SaleFinder catalogue belongs to. </param>
/// <param name="CurrencyCulture"> The culture used to display prices in the digest email. </param>
public record SaleFinderCatalogueDownloadInformation(
    int SaleId,
    Uri BaseUri,
    string Store,
    [property: JsonConverter(typeof(CultureInfoConverter))] CultureInfo CurrencyCulture
);
