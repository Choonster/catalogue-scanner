using Microsoft.AspNetCore.Components.Forms;
using System.ComponentModel.DataAnnotations;
using static CatalogueScanner.Core.MatchRule.SinglePropertyCatalogueItemMatchRule;

namespace CatalogueScanner.ConfigurationUI.ViewModel
{
    public class MatchRuleViewModel
    {
        public CatalogueItemProperty Property { get; set; }

        public PropertyMatchType MatchType { get; set; }

        [Required]
        public string Value { get; set; } = string.Empty;

        public bool InEditMode { get; set; }

        public EditContext EditContext { get; }

        public MatchRuleViewModel()
        {
            EditContext = new EditContext(this);
        }
    }
}
