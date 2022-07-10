using CatalogueScanner.ConfigurationUI.Extensions;
using CatalogueScanner.ConfigurationUI.ViewModel;
using CatalogueScanner.Core.Extensions;
using CatalogueScanner.Core.Options;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace CatalogueScanner.ConfigurationUI.Pages.Config
{
#pragma warning disable CA1724
    public partial class Matching
    {
        private MatchingOptions MatchingOptions => MatchingOptionsAccessor.Value;

        private ObservableCollection<BaseMatchRuleViewModel> matchRuleViewModels = new();

        protected override void OnInitialized()
        {
            matchRuleViewModels = new ObservableCollection<BaseMatchRuleViewModel>( MatchingOptions.Rules.ToViewModel());

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

        private async Task Save(MouseEventArgs _)
        {
            var options = MatchingOptions;
            var rules = options.Rules;

            rules.Clear();
            rules.AddRange(matchRuleViewModels.ToOptions());

            await MatchingOptionsSaver.SaveAsync(options).ConfigureAwait(true);

            NavigationManager.NavigateTo(NavigationManager.Uri, true);
        }
    }
}
