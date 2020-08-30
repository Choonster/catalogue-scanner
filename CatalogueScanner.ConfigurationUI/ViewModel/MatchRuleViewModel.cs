using CatalogueScanner.Core.Options;

namespace CatalogueScanner.ConfigurationUI.ViewModel
{
    public class MatchRuleViewModel
    {
        public CatalogueItemProperty Property { get; set; }
        public MatchType MatchType { get; set; }
        public string? Value { get; set; }
        public bool InEditMode { get; set; }
    }
}
