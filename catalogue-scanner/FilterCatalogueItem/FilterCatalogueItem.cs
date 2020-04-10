using CatalogueScanner.Dto.Config;
using CatalogueScanner.Dto.StorageEntity;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace CatalogueScanner
{
    /// <summary>
    /// Determines if a <see cref="CatalogueItem"/> matches any of the user-configured <see cref="CatalogueItemMatchRule"/> values and outputs the item if it does.
    /// </summary>
    public class FilterCatalogueItem
    {
        // Maps enum values to the corresponding class properties
        private static readonly IImmutableDictionary<CatalogueItemProperty, PropertyInfo> properties =
            Enum.GetValues(typeof(CatalogueItemProperty))
            .Cast<CatalogueItemProperty>()
            .ToImmutableDictionary(
                property => property,
                property => typeof(CatalogueItem).GetProperty(property.ToString())
            );

        private readonly List<CatalogueItemMatchRule> rules;

        public FilterCatalogueItem(IOptionsSnapshot<CatalogueScannerSettings> settings)
        {
            #region null checks
            if (settings is null)
            {
                throw new ArgumentNullException(nameof(settings));
            }
            #endregion

            rules = settings.Value.Rules;
        }

        [FunctionName("FilterCatalogueItem")]
        public void Run(
            [QueueTrigger(Constants.QueueNames.DownloadedItems)]
            CatalogueItem catalogueItem,
            ILogger log,
            [Queue(Constants.QueueNames.MatchedItems)]
            ICollector<CatalogueItem> collector
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

            if (collector is null)
            {
                throw new ArgumentNullException(nameof(collector));
            }
            #endregion

            if (rules.Any(rule => ItemMatches(catalogueItem, rule)))
            {
                collector.Add(catalogueItem);
            }
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
