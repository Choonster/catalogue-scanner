using CatalogueScanner.ConfigurationUI.ViewModel;
using CatalogueScanner.Core.MatchRule;
using CatalogueScanner.Core.Options;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CatalogueScanner.Core.MatchRule.SinglePropertyCatalogueItemMatchRule;

namespace CatalogueScanner.ConfigurationUI.Pages.Config
{
    public partial class Matching
    {
        private readonly PropertyMatchType[] matchTypes = Enum.GetValues(typeof(PropertyMatchType)).Cast<PropertyMatchType>().ToArray();
        private readonly CatalogueItemProperty[] properties = Enum.GetValues(typeof(CatalogueItemProperty)).Cast<CatalogueItemProperty>().ToArray();

        private MatchingOptions MatchingOptions => MatchingOptionsAccessor.Value;

        private List<MatchRuleViewModel> matchRules = new List<MatchRuleViewModel>();

        protected override void OnInitialized()
        {
            matchRules = OptionsToViewModel(MatchingOptions.Rules).ToList();

            base.OnInitialized();
        }

        private static IEnumerable<MatchRuleViewModel> OptionsToViewModel(List<ICatalogueItemMatchRule> matchRules)
        {
            return matchRules
                .Select(rule =>
                {
                    switch (rule.MatchRuleType)
                    {
                        case MatchRuleType.SingleProperty:
                            {
                                var singlePropertyMatchRule = (SinglePropertyCatalogueItemMatchRule)rule;

                                return new MatchRuleViewModel
                                {
                                    MatchType = singlePropertyMatchRule.MatchType,
                                    Property = singlePropertyMatchRule.Property,
                                    Value = singlePropertyMatchRule.Value,
                                };
                            }

                        case MatchRuleType.Compound:
                            return null!; // TODO: Handle compound rules

                        default:
                            throw new Exception($"Unkown MatchRuleType {rule.MatchRuleType}");
                    }
                });
        }

        private static IEnumerable<ICatalogueItemMatchRule> ViewModelToOptions(List<MatchRuleViewModel> matchRules)
        {
            return matchRules
                .Select(rule => new SinglePropertyCatalogueItemMatchRule
                {
                    MatchType = rule.MatchType,
                    Property = rule.Property,
                    Value = rule.Value,
                });
        }

        private void AddRule()
        {
            matchRules.Add(new MatchRuleViewModel
            {
                InEditMode = true,
            });
        }

        private void RemoveRule(MatchRuleViewModel matchRule)
        {
            matchRules.Remove(matchRule);
        }

        private async Task Save(MouseEventArgs args)
        {
            var options = MatchingOptions;
            var rules = options.Rules;

            rules.Clear();
            rules.AddRange(ViewModelToOptions(matchRules));

            await MatchingOptionsSaver.SaveAsync(options);

            NavigationManager.NavigateTo(NavigationManager.Uri, true);
        }
    }
}
