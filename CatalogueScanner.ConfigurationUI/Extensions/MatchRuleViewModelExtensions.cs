using CatalogueScanner.ConfigurationUI.ViewModel;
using CatalogueScanner.Core.Extensions;
using CatalogueScanner.Core.MatchRule;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CatalogueScanner.ConfigurationUI.Extensions
{
    public static class MatchRuleViewModelExtensions
    {
        public static IEnumerable<BaseMatchRuleViewModel> ToViewModel(this IEnumerable<ICatalogueItemMatchRule> matchRules) =>
            matchRules
                .Select<ICatalogueItemMatchRule, BaseMatchRuleViewModel>(rule =>
                {
                    switch (rule.MatchRuleType)
                    {
                        case MatchRuleType.SingleProperty:
                            {
                                var singlePropertyMatchRule = (SinglePropertyCatalogueItemMatchRule)rule;

                                return new SinglePropertyMatchRuleViewModel
                                {
                                    MatchType = singlePropertyMatchRule.MatchType,
                                    Property = singlePropertyMatchRule.Property,
                                    Value = singlePropertyMatchRule.Value,
                                };
                            }

                        case MatchRuleType.Compound:
                            {
                                var compoundMatchRule = (CompoundCatalogueItemMatchRule)rule;

                                var result = new CompoundMatchRuleViewModel
                                {
                                    MatchType = compoundMatchRule.MatchType,
                                };

                                result.ChildRules.AddRange(compoundMatchRule.ChildRules.ToViewModel());

                                return result;
                            }

                        default:
                            throw new InvalidOperationException($"Unkown MatchRuleType {rule.MatchRuleType}");
                    }
                });

        public static IEnumerable<ICatalogueItemMatchRule> ToOptions(this IEnumerable<BaseMatchRuleViewModel> matchRules) =>
            matchRules
                .Select<BaseMatchRuleViewModel, ICatalogueItemMatchRule>(rule =>
                {
                    switch (rule.MatchRuleType)
                    {
                        case MatchRuleType.SingleProperty:
                            {
                                var singlePropertyMatchRule = (SinglePropertyMatchRuleViewModel)rule;

                                return new SinglePropertyCatalogueItemMatchRule
                                {
                                    MatchType = singlePropertyMatchRule.MatchType,
                                    Property = singlePropertyMatchRule.Property,
                                    Value = singlePropertyMatchRule.Value,
                                };
                            }

                        case MatchRuleType.Compound:
                            {
                                var compoundMatchRule = (CompoundMatchRuleViewModel)rule;

                                var result = new CompoundCatalogueItemMatchRule
                                {
                                    MatchType = compoundMatchRule.MatchType,
                                };

                                result.ChildRules.AddRange(compoundMatchRule.ChildRules.ToOptions());

                                return result;
                            }

                        default:
                            throw new InvalidOperationException($"Unkown MatchRuleType {rule.MatchRuleType}");
                    }
                });
    }
}
