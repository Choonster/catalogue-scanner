using CatalogueScanner.Core.Dto.FunctionResult;
using CatalogueScanner.Core.MatchRule;
using CatalogueScanner.Core.Options;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
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
            if (optionsAccessor is null)
            {
                throw new ArgumentNullException(nameof(optionsAccessor));
            }
            #endregion

            rules = optionsAccessor.Value.Rules;
        }

        [FunctionName(CoreFunctionNames.FilterCatalogueItem)]
        public CatalogueItem? Run(
            [ActivityTrigger] CatalogueItem catalogueItem,
            ILogger log
        )
        {
            #region null checks
            if (catalogueItem is null)
            {
                throw new ArgumentNullException(nameof(catalogueItem));
            }

            if (log is null)
            {
                throw new ArgumentNullException(nameof(log));
            }
            #endregion

            return rules.Any(rule => rule.ItemMatches(catalogueItem)) ? catalogueItem : null;
        }
    }
}
