using CatalogueScanner.Core.Dto.FunctionResult;
using CatalogueScanner.Core.MatchRule;
using CatalogueScanner.Core.Options;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CatalogueScanner.Core.Functions
{
    /// <summary>
    /// Determines if a <see cref="CatalogueItem"/> matches any of the user-configured <see cref="ICatalogueItemMatchRule"/> values and outputs the item if it does, or null if it doesn't.
    /// </summary>
    public class FilterCatalogueItem
    {
        private readonly ICollection<ICatalogueItemMatchRule> rules;

        public FilterCatalogueItem(IOptionsSnapshot<MatchingOptions> optionsAccessor)
        {
            #region null checks
            ArgumentNullException.ThrowIfNull(optionsAccessor);
            #endregion

            rules = optionsAccessor.Value.Rules;
        }

        [Function(CoreFunctionNames.FilterCatalogueItem)]
        public CatalogueItem? Run(
            [ActivityTrigger] CatalogueItem catalogueItem
        )
        {
            #region null checks
            ArgumentNullException.ThrowIfNull(catalogueItem);
            #endregion

            return rules.Any(rule => rule.ItemMatches(catalogueItem)) ? catalogueItem : null;
        }
    }
}
