using CatalogueScanner.Core.Dto.FunctionResult;
using System;
using System.Text.RegularExpressions;

namespace CatalogueScanner.Core.MatchRule
{
    public class SinglePropertyCatalogueItemMatchRule : ICatalogueItemMatchRule
    {
        public MatchRuleType MatchRuleType => MatchRuleType.SingleProperty;

        public CatalogueItemProperty Property { get; set; }
        public PropertyMatchType MatchType { get; set; }
        public string Value { get; set; } = null!;

        public bool ItemMatches(CatalogueItem item)
        {
            var property = Property.GetPropertyInfo();

            if (MatchType.IsStringMatchType())
            {
                var propertyValue = property.GetValue(item)?.ToString();

                if (propertyValue is null)
                {
                    return false;
                }

                return MatchType switch
                {
                    PropertyMatchType.Exact => propertyValue == Value,
                    PropertyMatchType.Contains => propertyValue.Contains(Value, StringComparison.InvariantCulture),
                    PropertyMatchType.ContainsIgnoreCase => propertyValue.Contains(Value, StringComparison.InvariantCultureIgnoreCase),
                    PropertyMatchType.Regex => Regex.IsMatch(propertyValue, Value),
                    _ => throw new InvalidOperationException($"Unknown MatchType {MatchType}"),
                };
            }
            else if (MatchType.IsNumericMatchType())
            {
                var underlyingType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

                if (underlyingType == typeof(long))
                {
                    if (!long.TryParse(Value, out var ruleValue))
                    {
                        return false;
                    }

                    var propertyValue = (long)property.GetValue(item)!;

                    return MatchType switch
                    {
                        PropertyMatchType.LessThan => propertyValue < ruleValue,
                        PropertyMatchType.GreaterThan => propertyValue > ruleValue,
                        _ => throw new InvalidOperationException($"Unknown MatchType {MatchType}"),
                    };
                }
                else if (underlyingType == typeof(decimal))
                {
                    if (!decimal.TryParse(Value, out var ruleValue))
                    {
                        return false;
                    }

                    var propertyValue = (decimal)property.GetValue(item)!;

                    return MatchType switch
                    {
                        PropertyMatchType.LessThan => propertyValue < ruleValue,
                        PropertyMatchType.GreaterThan => propertyValue > ruleValue,
                        _ => throw new InvalidOperationException($"Unknown MatchType {MatchType}"),
                    };
                }
                else
                {
                    throw new InvalidOperationException($"Unsupported property type {property.PropertyType}");
                }
            }

            throw new InvalidOperationException($"Unknown MatchType {MatchType}");
        }

        public enum CatalogueItemProperty
        {
            Id,
            Sku,
            Uri,
            Name,
            Price,
            MultiBuyQuantity,
            MultiBuyTotalPrice,
        }

        public enum PropertyMatchType
        {
            Exact,
            Contains,
            ContainsIgnoreCase,
            Regex,
            LessThan,
            GreaterThan,
        }
    }
}
