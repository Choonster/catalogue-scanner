using CatalogueScanner.ConfigurationUI.ViewModel;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CatalogueScanner.Core.MatchRule.SinglePropertyCatalogueItemMatchRule;

namespace CatalogueScanner.ConfigurationUI.Pages.Config
{
    public partial class MatchRuleTable
    {
        private static readonly PropertyMatchType[] matchTypes = Enum.GetValues(typeof(PropertyMatchType)).Cast<PropertyMatchType>().ToArray();
        private static readonly CatalogueItemProperty[] properties = Enum.GetValues(typeof(CatalogueItemProperty)).Cast<CatalogueItemProperty>().ToArray();

        [Parameter]
        public List<BaseMatchRuleViewModel> MatchRules { get; set; } = new List<BaseMatchRuleViewModel>();

        private void RemoveRule(BaseMatchRuleViewModel matchRule)
        {
            MatchRules.Remove(matchRule);
        }

        private async Task OpenCompoundEditDialog(CompoundMatchRuleViewModel matchRule)
        {
            await JSRuntime.InvokeVoidAsync("alert", $"Compound edit for {matchRule.ChildRules.Count} rules!");
        }
    }
}
