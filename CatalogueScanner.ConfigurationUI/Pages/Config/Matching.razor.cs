﻿using CatalogueScanner.ConfigurationUI.Extensions;
using CatalogueScanner.ConfigurationUI.ViewModel;
using CatalogueScanner.Core.Options;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CatalogueScanner.ConfigurationUI.Pages.Config
{
    public partial class Matching
    {
        private MatchingOptions MatchingOptions => MatchingOptionsAccessor.Value;

        private List<BaseMatchRuleViewModel> matchRuleViewModels = new List<BaseMatchRuleViewModel>();

        protected override void OnInitialized()
        {
            matchRuleViewModels = MatchingOptions.Rules.ToViewModel().ToList();

            base.OnInitialized();
        }

        private void AddSinglePropertyRule()
        {
            matchRuleViewModels.Add(new SinglePropertyMatchRuleViewModel
            {
                InEditMode = true,
            });
        }

        private void AddCompoundRule()
        {
            var compoundRule = new CompoundMatchRuleViewModel();
            compoundRule.ChildRules.Add(new SinglePropertyMatchRuleViewModel
            {
                InEditMode = true,
            });

            matchRuleViewModels.Add(compoundRule);
        }

        private async Task Save(MouseEventArgs args)
        {
            var options = MatchingOptions;
            var rules = options.Rules;

            rules.Clear();
            rules.AddRange(matchRuleViewModels.ToOptions());

            await MatchingOptionsSaver.SaveAsync(options);

            NavigationManager.NavigateTo(NavigationManager.Uri, true);
        }
    }
}
