using CatalogueScanner.ConfigurationUI.ViewModel;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using static CatalogueScanner.Core.MatchRule.CompoundCatalogueItemMatchRule;
using static CatalogueScanner.Core.MatchRule.SinglePropertyCatalogueItemMatchRule;

namespace CatalogueScanner.ConfigurationUI.Pages.Config
{
    public partial class MatchRuleTable
    {
        private static readonly PropertyMatchType[] propertyMatchTypes = Enum.GetValues(typeof(PropertyMatchType)).Cast<PropertyMatchType>().ToArray();
        private static readonly CatalogueItemProperty[] properties = Enum.GetValues(typeof(CatalogueItemProperty)).Cast<CatalogueItemProperty>().ToArray();
        private static readonly CompoundMatchType[] compoundMatchTypes = Enum.GetValues(typeof(CompoundMatchType)).Cast<CompoundMatchType>().ToArray();

        private CompoundMatchRuleViewModel? dialogCompoundRule;

        [Parameter]
        public List<BaseMatchRuleViewModel> MatchRules { get; set; } = new List<BaseMatchRuleViewModel>();

        private void RemoveRule(BaseMatchRuleViewModel matchRule)
        {
            MatchRules.Remove(matchRule);
        }

        private void OpenCompoundEditDialog(CompoundMatchRuleViewModel matchRule)
        {
            dialogCompoundRule = matchRule;
        }

        private void CloseCompoundEditDialog()
        {
            dialogCompoundRule = null;
        }

        private void AddSinglePropertyRule()
        {
            #region null checks
            if (dialogCompoundRule is null)
            {
                throw new NullReferenceException($"{nameof(dialogCompoundRule)} is null");
            }
            #endregion

            dialogCompoundRule.ChildRules.Add(new SinglePropertyMatchRuleViewModel
            {
                InEditMode = true,
            });
        }

        private void AddCompoundRule()
        {
            #region null checks
            if (dialogCompoundRule is null)
            {
                throw new NullReferenceException($"{nameof(dialogCompoundRule)} is null");
            }
            #endregion

            var compoundRule = new CompoundMatchRuleViewModel();
            compoundRule.ChildRules.Add(new SinglePropertyMatchRuleViewModel
            {
                InEditMode = true,
            });

            dialogCompoundRule.ChildRules.Add(compoundRule);
        }
    }
}
