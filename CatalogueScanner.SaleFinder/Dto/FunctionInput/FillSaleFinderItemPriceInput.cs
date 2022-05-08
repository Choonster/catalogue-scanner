using CatalogueScanner.Core.Dto.FunctionResult;
using System.Globalization;

namespace CatalogueScanner.SaleFinder.Dto.FunctionInput
{
    public record FillSaleFinderItemPriceInput(CultureInfo CurrencyCulture, CatalogueItem Item)
    {
    }
}
