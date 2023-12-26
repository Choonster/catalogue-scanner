using CatalogueScanner.Core.Dto.FunctionResult;

namespace CatalogueScanner.Core.MatchRule;

public interface ICatalogueItemMatchRule
{
    MatchRuleType MatchRuleType { get; }

    bool ItemMatches(CatalogueItem item);
}

public enum MatchRuleType
{
    SingleProperty,
    Compound,
}
