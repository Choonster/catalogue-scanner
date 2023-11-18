using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace CatalogueScanner.Core.MatchRule;

public interface ICatalogueItemMatchRuleSerialiser
{
    IEnumerable<ICatalogueItemMatchRule> DeserialiseMatchRules(IConfigurationSection rulesConfig);
}