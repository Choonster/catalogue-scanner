using static CatalogueScanner.Core.MatchRule.SinglePropertyCatalogueItemMatchRule;

namespace CatalogueScanner.Core.MatchRule;

public static class PropertyMatchTypeExtensions
{
    public static bool IsStringMatchType(this PropertyMatchType matchType) =>
        matchType is PropertyMatchType.Exact or PropertyMatchType.Contains or PropertyMatchType.ContainsIgnoreCase or PropertyMatchType.Regex;

    public static bool IsNumericMatchType(this PropertyMatchType matchType) =>
        matchType is PropertyMatchType.LessThan or PropertyMatchType.GreaterThan;
}
