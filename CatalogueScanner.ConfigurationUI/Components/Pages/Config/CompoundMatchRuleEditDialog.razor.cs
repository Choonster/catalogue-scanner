using CatalogueScanner.ConfigurationUI.ViewModel;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using static CatalogueScanner.Core.MatchRule.CompoundCatalogueItemMatchRule;

namespace CatalogueScanner.ConfigurationUI.Components.Pages.Config;

public partial class CompoundMatchRuleEditDialog
{
    private static readonly CompoundMatchType[] compoundMatchTypes = Enum.GetValues(typeof(CompoundMatchType)).Cast<CompoundMatchType>().ToArray();

    [Parameter]
    public CompoundMatchRuleViewModel CompoundRule { get; set; } = new CompoundMatchRuleViewModel();

    [CascadingParameter]
    public MudDialogInstance DialogInstance { get; set; } = null!;

    protected override void OnInitialized()
    {
        #region null checks
        if (DialogInstance is null)
        {
            throw new InvalidOperationException($"{nameof(DialogInstance)} is null");
        }
        #endregion
    }

    private void Close()
    {
        DialogInstance.Close();
    }

    private void AddSinglePropertyRule()
    {
        CompoundRule.ChildRules.Add(new SinglePropertyMatchRuleViewModel
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

        CompoundRule.ChildRules.Add(compoundRule);
    }
}
