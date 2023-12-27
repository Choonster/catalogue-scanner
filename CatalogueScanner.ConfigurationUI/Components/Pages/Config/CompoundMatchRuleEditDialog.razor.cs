using CatalogueScanner.ConfigurationUI.ViewModel;
using MatBlazor;
using Microsoft.AspNetCore.Components;
using static CatalogueScanner.Core.MatchRule.CompoundCatalogueItemMatchRule;

namespace CatalogueScanner.ConfigurationUI.Components.Pages.Config;

public partial class CompoundMatchRuleEditDialog
{
    private static readonly CompoundMatchType[] compoundMatchTypes = Enum.GetValues(typeof(CompoundMatchType)).Cast<CompoundMatchType>().ToArray();

    [Parameter]
    public CompoundMatchRuleViewModel CompoundRule { get; set; } = new CompoundMatchRuleViewModel();

    [CascadingParameter]
    public MatDialogReference DialogReference { get; set; } = null!;

    protected override void OnInitialized()
    {
        #region null checks
        if (DialogReference is null)
        {
            throw new InvalidOperationException($"{nameof(DialogReference)} is null");
        }
        #endregion
    }

    private void Close()
    {
        DialogReference.Close(null);
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
