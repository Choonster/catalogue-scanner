using CatalogueScanner.Core.MatchRule;
using Microsoft.AspNetCore.Components.Forms;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static CatalogueScanner.Core.MatchRule.CompoundCatalogueItemMatchRule;
using static CatalogueScanner.Core.MatchRule.SinglePropertyCatalogueItemMatchRule;

namespace CatalogueScanner.ConfigurationUI.ViewModel
{
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
        public override MatchRuleType MatchRuleType => MatchRuleType.SingleProperty;

        public bool InEditMode { get; set; }

        public CatalogueItemProperty Property { get; set; }

        public PropertyMatchType MatchType { get; set; }

        [Required]
        public string Value { get; set; } = string.Empty;
    }

    public class CompoundMatchRuleViewModel : BaseMatchRuleViewModel
    {
        public override MatchRuleType MatchRuleType => MatchRuleType.Compound;

        public CompoundMatchType MatchType { get; set; }

        public ICollection<BaseMatchRuleViewModel> ChildRules { get; } = new List<BaseMatchRuleViewModel>();
    }
}
