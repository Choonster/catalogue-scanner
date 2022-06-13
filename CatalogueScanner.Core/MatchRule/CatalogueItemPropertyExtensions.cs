using CatalogueScanner.Core.Dto.FunctionResult;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using static CatalogueScanner.Core.MatchRule.SinglePropertyCatalogueItemMatchRule;

namespace CatalogueScanner.Core.MatchRule
{
    public static class CatalogueItemPropertyExtensions
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

        private static readonly IImmutableSet<CatalogueItemProperty> numericProperties = properties
            .Where(pair => IsNumericType(GetUnderlyingType(pair.Value.PropertyType)))
            .Select(pair => pair.Key)
            .ToImmutableHashSet();

        public static PropertyInfo GetPropertyInfo(this CatalogueItemProperty property) => properties[property];

        public static bool IsNumericProperty(this CatalogueItemProperty property) => numericProperties.Contains(property);

        private static bool IsNumericType(Type type) => type == typeof(long) || type == typeof(decimal);

        private static Type GetUnderlyingType(Type type) => Nullable.GetUnderlyingType(type) ?? type;
    }
}
