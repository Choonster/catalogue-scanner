namespace CatalogueScanner.Core.Options
{
    public class CatalogueItemMatchRule
    {
        public CatalogueItemProperty Property { get; set; }
        public MatchType MatchType { get; set; }
        public string Value { get; set; } = null!;
    }

    public enum CatalogueItemProperty
    {
        Id,
        Sku,
        Uri,
        Name,
    }

    public enum MatchType
    {
        Exact,
        Contains,
        ContainsIgnoreCase,
        Regex,
    }
}
