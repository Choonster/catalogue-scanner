using CatalogueScanner.Core.Dto.FunctionResult;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace CatalogueScanner.Core.MatchRule
{
    public class SinglePropertyCatalogueItemMatchRule : ICatalogueItemMatchRule
    {
        public MatchRuleType MatchRuleType => MatchRuleType.SingleProperty;

        public CatalogueItemProperty Property { get; set; }
        public PropertyMatchType MatchType { get; set; }
        public string Value { get; set; } = null!;


        // Maps enum values to the corresponding class properties
        private static readonly IImmutableDictionary<CatalogueItemProperty, PropertyInfo> properties =
            Enum.GetValues(typeof(CatalogueItemProperty))
            .Cast<CatalogueItemProperty>()
            .ToImmutableDictionary(
                property => property,
                property => typeof(CatalogueItem).GetProperty(property.ToString()) ??
                    throw new InvalidOperationException($"Couldn't find property with name \"{property}\" on type {typeof(CatalogueItem).FullName}")
            );

        public bool ItemMatches(CatalogueItem item)
        {
            var property = properties[Property];
            var value = property.GetValue(item)?.ToString();

            if (value is null)
            {
                return false;
            }

            return MatchType switch
            {
                PropertyMatchType.Exact => value == Value,
                PropertyMatchType.Contains => value.Contains(Value, StringComparison.InvariantCulture),
                PropertyMatchType.ContainsIgnoreCase => value.Contains(Value, StringComparison.InvariantCultureIgnoreCase),
                PropertyMatchType.Regex => Regex.IsMatch(value, Value),
                _ => throw new InvalidOperationException($"Unknown MatchType {MatchType}"),
            };
        }

        public enum CatalogueItemProperty
        {
            Id,
            Sku,
            Uri,
            Name,
        }

        public enum PropertyMatchType
        {
            Exact,
            Contains,
            ContainsIgnoreCase,
            Regex,
        }
    }
}
