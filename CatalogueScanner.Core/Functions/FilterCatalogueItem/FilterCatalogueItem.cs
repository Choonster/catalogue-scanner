using CatalogueScanner.Core.Config;
using CatalogueScanner.Core.Dto.FunctionResult;
using CatalogueScanner.Core.Options;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace CatalogueScanner.Core.Functions
{
    /// <summary>
    /// Determines if a <see cref="CatalogueItem"/> matches any of the user-configured <see cref="CatalogueItemMatchRule"/> values and outputs the item if it does, or null if it doesn't.
    /// </summary>
    public class FilterCatalogueItem
    {
        // Maps enum values to the corresponding class properties
        private static readonly IImmutableDictionary<CatalogueItemProperty, PropertyInfo> properties =
            Enum.GetValues(typeof(CatalogueItemProperty))
            .Cast<CatalogueItemProperty>()
            .ToImmutableDictionary(
                property => property,
                property => typeof(CatalogueItem).GetProperty(property.ToString()) ??
                    throw new InvalidOperationException($"Couldn't find property with name \"{property}\" on type {typeof(CatalogueItem).FullName}")
            );

        private readonly List<CatalogueItemMatchRule> rules;

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

            return rules.Any(rule => ItemMatches(catalogueItem, rule)) ? catalogueItem : null;
        }

        private static bool ItemMatches(CatalogueItem item, CatalogueItemMatchRule rule)
        {
            var property = properties[rule.Property];
            var value = property.GetValue(item)?.ToString();

            if (value is null)
            {
                return false;
            }

            return rule.MatchType switch
            {
                MatchType.Exact => value == rule.Value,
                MatchType.Contains => value.Contains(rule.Value, StringComparison.InvariantCulture),
                MatchType.ContainsIgnoreCase => value.Contains(rule.Value, StringComparison.InvariantCultureIgnoreCase),
                MatchType.Regex => Regex.IsMatch(value, rule.Value),
                _ => throw new InvalidOperationException($"Unknown MatchType {rule.MatchType}"),
            };
        }
    }
}
