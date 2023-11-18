using CatalogueScanner.ConfigurationUI.ViewModel;
using CatalogueScanner.Core.MatchRule;
using MatBlazor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using static CatalogueScanner.Core.MatchRule.SinglePropertyCatalogueItemMatchRule;

namespace CatalogueScanner.ConfigurationUI.Pages.Config
{
    public partial class MatchRuleTable : ComponentBase
    {
        private static readonly PropertyMatchType[] allPropertyMatchTypes = Enum.GetValues(typeof(PropertyMatchType)).Cast<PropertyMatchType>().ToArray();
        private static readonly PropertyMatchType[] stringPropertyMatchType = allPropertyMatchTypes.Where(matchType => matchType.IsStringMatchType()).ToArray();
        private static readonly CatalogueItemProperty[] properties = Enum.GetValues(typeof(CatalogueItemProperty)).Cast<CatalogueItemProperty>().ToArray();

        [Parameter]
        public ObservableCollection<BaseMatchRuleViewModel> MatchRules { get; set; } = new ObservableCollection<BaseMatchRuleViewModel>();

        protected override void OnParametersSet()
        {
            InitialiseMatchRules(MatchRules);

            MatchRules.CollectionChanged += MatchRules_CollectionChanged;
        }

        private void MatchRules_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            #region null checks
            ArgumentNullException.ThrowIfNull(sender);

            ArgumentNullException.ThrowIfNull(e);
            #endregion

            var matchRules = (ObservableCollection<BaseMatchRuleViewModel>)sender;

            if (e.NewItems != null)
            {
                InitialiseMatchRules(e.NewItems);
            }

            if (e.OldItems != null)
            {
                foreach (var matchRule in e.OldItems.OfType<SinglePropertyMatchRuleViewModel>())
                {
                    matchRule.EditContext.OnFieldChanged -= EditContext_OnFieldChanged;
                }
            }
        }

        private void InitialiseMatchRules(IEnumerable matchRules)
        {
            foreach (var matchRule in matchRules.OfType<SinglePropertyMatchRuleViewModel>())
            {
                matchRule.EditContext.OnFieldChanged += EditContext_OnFieldChanged;
                SetMatchTypes(matchRule);
            }
        }

        private void EditContext_OnFieldChanged(object? sender, FieldChangedEventArgs e)
        {
            #region null checks
            ArgumentNullException.ThrowIfNull(sender);

            ArgumentNullException.ThrowIfNull(e);
            #endregion

            var editContext = (EditContext)sender;

            if (e.FieldIdentifier.FieldName == nameof(SinglePropertyMatchRuleViewModel.Property))
            {
                // Delay updating the match types to work around error in MatSelectItem
                InvokeAsync(() => SetMatchTypes((SinglePropertyMatchRuleViewModel)editContext.Model));
            }
        }

        private static void SetMatchTypes(SinglePropertyMatchRuleViewModel matchRule)
        {
            matchRule.MatchTypes = matchRule.Property.IsNumericProperty() ? allPropertyMatchTypes : stringPropertyMatchType;
        }

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
