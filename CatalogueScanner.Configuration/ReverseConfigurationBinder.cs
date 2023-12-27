using Microsoft.Extensions.Configuration;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace CatalogueScanner.Configuration;

/// <summary>
/// Acts as the reverse of <see cref="ConfigurationBinder"/>,
/// converting an <see cref="Microsoft.Extensions.Options.IOptions{}"/> instance to a <see cref="Dictionary{string,string}"/>;
/// suitable for sending to Azure App Configuration.
/// 
/// Code adapted from <see cref="ConfigurationBinder"/>.
/// </summary>
internal static class ReverseConfigurationBinder
{
    private const BindingFlags DeclaredOnlyLookup = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

    public static Dictionary<string, string?> GetConfigurationSettings<TOptions>(TOptions options, IConfigurationSection configuration)
    {
        var settings = new Dictionary<string, string?>();

        FillInstance(typeof(TOptions), options, configuration, settings);

        return settings;
    }

    private static void FillInstance(Type type, object? instance, IConfigurationSection config, Dictionary<string, string?> settings)
    {
        if (instance is null)
        {
            return;
        }

        if (IsSupportedSimpleType(type))
        {
            TryConvertValue(type, instance, config.Path, out var convertedValue, out var error);

            if (error != null)
            {
                throw error;
            }

            settings.Add(config.Path, convertedValue);

            return;
        }

        // See if its a Dictionary
        var collectionInterface = FindOpenGenericInterface(typeof(IDictionary<,>), type);
        if (collectionInterface != null)
        {
            FillDictionary(instance, collectionInterface, config, settings);
        }
        else if (type.IsArray)
        {
            FillArray((Array)instance, config, settings);
        }
        else
        {
            // See if its an ICollection
            collectionInterface = FindOpenGenericInterface(typeof(ICollection<>), type);
            if (collectionInterface != null)
            {
                FillCollection(instance, collectionInterface, config, settings);
            }
            // Something else
            else
            {
                FillNonScalar(config, instance, settings);
            }
        }
    }

    private static bool TryConvertValue(Type type, object? value, string path, out string? result, out Exception? error)
    {
        error = null;
        result = null;

        if (type == typeof(string))
        {
            result = (string?)value;
            return true;
        }

        if (IsNullable(type))
        {
            if (value is null)
            {
                return true;
            }

            return TryConvertValue(Nullable.GetUnderlyingType(type)!, value, path, out result, out error);
        }

        var converter = TypeDescriptor.GetConverter(type);
        if (converter.CanConvertTo(typeof(string)))
        {
            try
            {
                result = converter.ConvertToInvariantString(value);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                error = new InvalidOperationException($"Failed to convert configuration value at '{path}' from type '{type}'.", ex);
            }
            return true;
        }

        return false;
    }

    private static void FillArray(Array source, IConfiguration config, Dictionary<string, string?> settings)
    {
        if (source is null)
        {
            return;
        }

        var elementType = source.GetType().GetElementType()!;

        for (int i = 0; i < source.Length; i++)
        {
            try
            {
                FillInstance(
                    type: elementType,
                    instance: source.GetValue(i),
                    config: config.GetSection(i.ToString(CultureInfo.InvariantCulture)),
                    settings: settings
                );
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch
#pragma warning restore CA1031 // Do not catch general exception types
            {
            }
        }
    }

    private static void FillCollection(object collection, Type collectionType, IConfiguration config, Dictionary<string, string?> settings)
    {
        if (collection is null)
        {
            return;
        }

        // ICollection<T> is guaranteed to have exactly one parameter
        var itemType = collectionType.GenericTypeArguments[0];

        var index = 0;

        foreach (var item in (IEnumerable)collection)
        {
            try
            {
                var section = config.GetSection(index.ToString(CultureInfo.InvariantCulture));

                FillInstance(
                    type: itemType,
                    instance: item,
                    config: section,
                    settings: settings
                );
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch
#pragma warning restore CA1031 // Do not catch general exception types
            {
            }

            index++;
        }
    }

    private static void FillDictionary(object dictionary, Type dictionaryType, IConfiguration config, Dictionary<string, string?> settings)
    {
        // IDictionary<K,V> is guaranteed to have exactly two parameters
        var keyType = dictionaryType.GenericTypeArguments[0];
        var valueType = dictionaryType.GenericTypeArguments[1];
        var keyTypeIsEnum = keyType.IsEnum;

        if (keyType != typeof(string) && !keyTypeIsEnum)
        {
            // We only support string and enum keys
            return;
        }

        var keyValuePairType = typeof(KeyValuePair<,>).MakeGenericType(keyType, valueType);
        var keyGetter = keyValuePairType.GetProperty(nameof(KeyValuePair<object, object>.Key), DeclaredOnlyLookup)!;
        var valueGetter = keyValuePairType.GetProperty(nameof(KeyValuePair<object, object>.Value), DeclaredOnlyLookup)!;

        foreach (var pair in (IEnumerable)dictionary)
        {
            var key = keyGetter.GetValue(pair)!.ToString()!;
            var child = config.GetSection(key);

            FillInstance(
               type: valueType,
               instance: valueGetter.GetValue(pair),
               config: child,
               settings: settings
            );
        }
    }

    private static void FillNonScalar(IConfiguration configuration, object instance, Dictionary<string, string?> settings)
    {
        if (instance != null)
        {
            foreach (var property in GetAllProperties(instance.GetType()))
            {
                FillProperty(property, instance, configuration, settings);
            }
        }
    }

    private static void FillProperty(PropertyInfo property, object instance, IConfiguration config, Dictionary<string, string?> settings)
    {
        // We don't support set only, non public, or indexer properties
        if (property.GetMethod == null ||
            !property.GetMethod.IsPublic ||
            property.GetMethod.GetParameters().Length > 0)
        {
            return;
        }

        var propertyValue = property.GetValue(instance);

        if (propertyValue is null)
        {
            return;
        }

        FillPropertyValue(property, instance, config, settings);
    }

    private static Type? FindOpenGenericInterface(Type expected, Type actual)
    {
        if (actual.IsGenericType &&
            actual.GetGenericTypeDefinition() == expected)
        {
            return actual;
        }

        var interfaces = actual.GetInterfaces();
        foreach (var interfaceType in interfaces)
        {
            if (interfaceType.IsGenericType &&
                interfaceType.GetGenericTypeDefinition() == expected)
            {
                return interfaceType;
            }
        }
        return null;
    }

    private static List<PropertyInfo> GetAllProperties(Type type)
    {
        var allProperties = new List<PropertyInfo>();

        do
        {
            allProperties.AddRange(type.GetProperties(DeclaredOnlyLookup));
            type = type.BaseType!;
        }
        while (type != typeof(object));

        return allProperties;
    }

    private static bool IsSupportedSimpleType(Type type)
    {
        return IsNonPointerPrimitive(type) ||
            (IsNullable(type) && IsNonPointerPrimitive(Nullable.GetUnderlyingType(type)!)) ||
            type == typeof(string) ||
            type.IsEnum;
    }

    private static bool IsNonPointerPrimitive(Type type)
    {
        return type.IsPrimitive && type != typeof(IntPtr) && type != typeof(UIntPtr);
    }

    private static bool IsNullable(Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
    }

    private static void FillPropertyValue(PropertyInfo property, object instance, IConfiguration config, Dictionary<string, string?> settings)
    {
        var propertyName = GetPropertyName(property);

        FillInstance(
            property.PropertyType,
            property.GetValue(instance),
            config.GetSection(propertyName),
            settings
        );
    }

    private static string GetPropertyName(MemberInfo property)
    {
        ArgumentNullException.ThrowIfNull(property);

        // Check for a custom property name used for configuration key binding
        foreach (var attributeData in property.GetCustomAttributesData())
        {
            if (attributeData.AttributeType != typeof(ConfigurationKeyNameAttribute))
            {
                continue;
            }

            // Ensure ConfigurationKeyName constructor signature matches expectations
            if (attributeData.ConstructorArguments.Count != 1)
            {
                break;
            }

            // Assumes ConfigurationKeyName constructor first arg is the string key name
            var name = attributeData
                .ConstructorArguments[0]
                .Value?
                .ToString();

            return !string.IsNullOrWhiteSpace(name) ? name : property.Name;
        }

        return property.Name;
    }
}
