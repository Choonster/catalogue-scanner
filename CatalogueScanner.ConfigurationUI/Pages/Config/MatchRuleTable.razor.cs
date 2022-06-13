using CatalogueScanner.ConfigurationUI.ViewModel;
using CatalogueScanner.Core.MatchRule;
using MatBlazor;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CatalogueScanner.Core.MatchRule.SinglePropertyCatalogueItemMatchRule;

namespace CatalogueScanner.ConfigurationUI.Pages.Config
{
    public partial class MatchRuleTable
    {
        private static readonly PropertyMatchType[] allPropertyMatchTypes = Enum.GetValues(typeof(PropertyMatchType)).Cast<PropertyMatchType>().ToArray();
        private static readonly PropertyMatchType[] stringPropertyMatchType = allPropertyMatchTypes.Where(matchType => matchType.IsStringMatchType()).ToArray();
        private static readonly CatalogueItemProperty[] properties = Enum.GetValues(typeof(CatalogueItemProperty)).Cast<CatalogueItemProperty>().ToArray();
        
        [Parameter]
        public ICollection<BaseMatchRuleViewModel> MatchRules { get; set; } = new List<BaseMatchRuleViewModel>();

        private void RemoveRule(BaseMatchRuleViewModel matchRule)
        {
            MatchRules.Remove(matchRule);
        }

        private async Task OpenCompoundEditDialog(CompoundMatchRuleViewModel matchRule)
        {
            await DialogService.OpenFullPageAsync(typeof(CompoundMatchRuleEditDialog), new MatDialogOptions
            {
                Attributes = new Dictionary<string, object>
                {
                    [nameof(CompoundMatchRuleEditDialog.CompoundRule)] = matchRule,
                },
            }).ConfigureAwait(true);
        }
    }
}
