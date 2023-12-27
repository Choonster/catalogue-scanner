using CatalogueScanner.Core.MatchRule;

namespace CatalogueScanner.Core.Options;

public class MatchingOptions
{
    public const string Matching = "Matching";

    public ICollection<ICatalogueItemMatchRule> Rules { get; } = new List<ICatalogueItemMatchRule>();
}
