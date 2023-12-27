using CatalogueScanner.Core.Dto.FunctionResult;

namespace CatalogueScanner.Core.MatchRule;

public class CompoundCatalogueItemMatchRule : ICatalogueItemMatchRule
{
    public MatchRuleType MatchRuleType => MatchRuleType.Compound;

    public CompoundMatchType MatchType { get; set; }

    public ICollection<ICatalogueItemMatchRule> ChildRules { get; } = new List<ICatalogueItemMatchRule>();

    public bool ItemMatches(CatalogueItem item) =>
        MatchType switch
        {
            CompoundMatchType.And => ChildRules.All(rule => rule.ItemMatches(item)),
            CompoundMatchType.Or => ChildRules.Any(rule => rule.ItemMatches(item)),
            CompoundMatchType.Not => !ChildRules.Any(rule => rule.ItemMatches(item)),
            _ => throw new InvalidOperationException($"Unkown MatchType {MatchType}")
        };

    public enum CompoundMatchType
    {
        And,
        Or,
        Not,
    }
}
