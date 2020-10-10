using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CatalogueScanner.Core.MatchRule
{
    public class CatalogueItemMatchRuleSerialiser
    {
        /// <summary>
        /// Parses a collection of <see cref="ICatalogueItemMatchRule"/> values from the supplied <see cref="IConfigurationSection"/>.
        /// 
        /// <paramref name="rulesConfig"/> should contain integer keys starting from 0, each containing the appropriate <see cref="IConfigurationSection"/> properties.
        /// </summary>
        /// <example>
        /// <list type="table">
        /// <item>0:MatchRuleType = SingleProperty</item>
        /// <item>0:MatchType = ContainsIgnoreCase</item>
        /// <item>0:Property = Name</item>
        /// <item>0:Value = Foobar</item>
        /// <item>1:MatchRuleType = Compound</item>
        /// <item>1:MatchType = Or</item>
        /// <item>1:ChildRules:0:MatchRuleType = SingleProperty</item>
        /// <item>1:ChildRules:0:MatchType = Regex</item>
        /// <item>1:ChildRules:0:Property = Id</item>
        /// <item>1:ChildRules:0:Value = Foo.+Bar</item>
        /// <item>1:ChildRules:1:MatchRuleType = SingleProperty</item>
        /// <item>1:ChildRules:1:MatchType = Exact</item>
        /// <item>1:ChildRules:1:Property = Sku</item>
        /// <item>1:ChildRules:1:Value = FoobarBaz</item>
        /// </list>
        /// </example>
        /// <param name="rulesConfig"></param>
        /// <returns></returns>
        public IEnumerable<ICatalogueItemMatchRule> DeserialiseMatchRules(IConfigurationSection rulesConfig)
        {
            #region null checks
            if (rulesConfig is null)
            {
                throw new ArgumentNullException(nameof(rulesConfig));
            }
            #endregion

            return rulesConfig
                   .GetChildren()
                   .Where(childSection => int.TryParse(childSection.Key, out _))
                   .OrderBy(childSection => int.Parse(childSection.Key, CultureInfo.InvariantCulture))
                   .Select<IConfigurationSection, ICatalogueItemMatchRule>(childSection =>
                   {
                       var matchRuleType = childSection.GetValue<MatchRuleType>("MatchRuleType");

                       switch (matchRuleType)
                       {
                           case MatchRuleType.SingleProperty:
                               return childSection.Get<SinglePropertyCatalogueItemMatchRule>();

                           case MatchRuleType.Compound:
                               {
                                   var matchRule = childSection.Get<CompoundCatalogueItemMatchRule>();

                                   matchRule.ChildRules.AddRange(DeserialiseMatchRules(childSection.GetSection("ChildRules")));

                                   return matchRule;
                               }

                           default:
                               throw new InvalidOperationException($"Unkown MatchRuleType {matchRuleType}");
                       }
                   });
        }
    }
}
