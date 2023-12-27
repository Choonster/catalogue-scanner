using CatalogueScanner.Core.MatchRule;
using Microsoft.AspNetCore.Components.Forms;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using static CatalogueScanner.Core.MatchRule.CompoundCatalogueItemMatchRule;
using static CatalogueScanner.Core.MatchRule.SinglePropertyCatalogueItemMatchRule;

namespace CatalogueScanner.ConfigurationUI.ViewModel;

public abstract class BaseMatchRuleViewModel
{
    public abstract MatchRuleType MatchRuleType { get; }

    public EditContext EditContext { get; }

    protected BaseMatchRuleViewModel()
    {
        EditContext = new EditContext(this);
    }
}

public class SinglePropertyMatchRuleViewModel : BaseMatchRuleViewModel
{
    private CatalogueItemProperty property;

    public IReadOnlyList<PropertyMatchType>? MatchTypes { get; set; }

    public override MatchRuleType MatchRuleType => MatchRuleType.SingleProperty;

    public bool InEditMode { get; set; }

    public CatalogueItemProperty Property
    {
        get => property;
        set
        {
            property = value;

            OnPropertyChanged();
        }
    }

    public PropertyMatchType MatchType { get; set; }

    [Required]
    public string Value { get; set; } = string.Empty;

    private void OnPropertyChanged()
    {
        if (MatchType.IsNumericMatchType() && !property.IsNumericProperty())
        {
            MatchType = PropertyMatchType.Exact;
        }
    }
}

public class CompoundMatchRuleViewModel : BaseMatchRuleViewModel
{
    public override MatchRuleType MatchRuleType => MatchRuleType.Compound;

    public CompoundMatchType MatchType { get; set; }

    public ObservableCollection<BaseMatchRuleViewModel> ChildRules { get; } = [];
}
