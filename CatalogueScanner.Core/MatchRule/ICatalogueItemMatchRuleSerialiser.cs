using Microsoft.Extensions.Configuration;

namespace CatalogueScanner.Core.MatchRule;

public interface ICatalogueItemMatchRuleSerialiser
{
    IEnumerable<ICatalogueItemMatchRule> DeserialiseMatchRules(IConfigurationSection rulesConfig);
}