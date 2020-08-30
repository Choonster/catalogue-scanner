using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace CatalogueScanner.Configuration
{
    /// <summary>
    /// Acts as the reverse of <see cref="ConfigurationBinder"/>,
    /// converting an <see cref="Microsoft.Extensions.Options.IOptions{}"/> instance to a <see cref="Dictionary{string,string}"/>;
    /// suitable for sending to Azure App Configuration.
    /// 
    /// Code adapted from <see cref="ConfigurationBinder"/>.
    /// </summary>
    class ReverseConfigurationBinder
    {
        public static Dictionary<string, string> GetConfigurationSettings<TOptions>(TOptions options, IConfigurationSection configuration)
        {
            var settings = new Dictionary<string, string>();

            FillInstance(typeof(TOptions), options, configuration, settings);

            return settings;
        }

        private static void FillInstance(Type type, object instance, IConfigurationSection config, Dictionary<string, string> settings)
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

        private static bool TryConvertValue(Type type, object value, string path, out string result, out Exception error)
        {
            error = null;
            result = null;

            if (type == typeof(string))
            {
                result = (string)value;
                return true;
            }

            if (IsNullable(type))
            {
                if (value is null)
                {
                    return true;
                }

                return TryConvertValue(Nullable.GetUnderlyingType(type), value, path, out result, out error);
            }

            var converter = TypeDescriptor.GetConverter(type);
            if (converter.CanConvertTo(typeof(string)))
            {
                try
                {
                    result = converter.ConvertToInvariantString(value);
                }
                catch (Exception ex)
                {
                    error = new InvalidOperationException($"Failed to convert configuration value at '{path}' from type '{type}'.", ex);
                }
                return true;
            }

            return false;
        }

        private static void FillArray(Array source, IConfiguration config, Dictionary<string, string> settings)
        {
            if (source is null)
            {
                return;
            }

            var elementType = source.GetType().GetElementType();

            for (int i = 0; i < source.Length; i++)
            {
                try
                {
                    FillInstance(
                        type: elementType,
                        instance: source.GetValue(i),
                        config: config.GetSection(i.ToString()),
                        settings: settings
                    );
                }
                catch
                {
                }
            }
        }

        private static void FillCollection(object collection, Type collectionType, IConfiguration config, Dictionary<string, string> settings)
        {
            if (collection is null)
            {
                return;
            }

            var typeInfo = collectionType.GetTypeInfo();

            // ICollection<T> is guaranteed to have exactly one parameter
            var itemType = typeInfo.GenericTypeArguments[0];

            var index = 0;

            foreach (var item in (IEnumerable)collection)
            {
                try
                {
                    var section = config.GetSection(index.ToString());

                    FillInstance(
                        type: itemType,
                        instance: item,
                        config: section,
                        settings: settings
                    );
                }
                catch
                {
                }

                index++;
            }
        }

        private static void FillDictionary(object dictionary, Type dictionaryType, IConfiguration config, Dictionary<string, string> settings)
        {
            var typeInfo = dictionaryType.GetTypeInfo();

            // IDictionary<K,V> is guaranteed to have exactly two parameters
            var keyType = typeInfo.GenericTypeArguments[0];
            var valueType = typeInfo.GenericTypeArguments[1];
            var keyTypeIsEnum = keyType.GetTypeInfo().IsEnum;

            if (keyType != typeof(string) && !keyTypeIsEnum)
            {
                // We only support string and enum keys
                return;
            }

            var keyValuePairType = typeof(KeyValuePair<,>).MakeGenericType(keyType, valueType).GetTypeInfo();
            var keyGetter = keyValuePairType.GetDeclaredProperty(nameof(KeyValuePair<object, object>.Key));
            var valueGetter = keyValuePairType.GetDeclaredProperty(nameof(KeyValuePair<object, object>.Value));

            foreach (var pair in (IEnumerable)dictionary)
            {
                var key = keyGetter.GetValue(pair).ToString();
                var child = config.GetSection(key);

                FillInstance(
                   type: valueType,
                   instance: valueGetter.GetValue(pair),
                   config: child,
                   settings: settings
                );
            }
        }

        private static void FillNonScalar(IConfiguration configuration, object instance, Dictionary<string, string> settings)
        {
            if (instance != null)
            {
                foreach (var property in GetAllProperties(instance.GetType().GetTypeInfo()))
                {
                    FillProperty(property, instance, configuration, settings);
                }
            }
        }

        private static void FillProperty(PropertyInfo property, object instance, IConfiguration config, Dictionary<string, string> settings)
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

            FillInstance(property.PropertyType, propertyValue, config.GetSection(property.Name), settings);
        }

        private static Type FindOpenGenericInterface(Type expected, Type actual)
        {
            var actualTypeInfo = actual.GetTypeInfo();
            if (actualTypeInfo.IsGenericType &&
                actual.GetGenericTypeDefinition() == expected)
            {
                return actual;
            }

            var interfaces = actualTypeInfo.ImplementedInterfaces;
            foreach (var interfaceType in interfaces)
            {
                if (interfaceType.GetTypeInfo().IsGenericType &&
                    interfaceType.GetGenericTypeDefinition() == expected)
                {
                    return interfaceType;
                }
            }
            return null;
        }

        private static IEnumerable<PropertyInfo> GetAllProperties(TypeInfo type)
        {
            var allProperties = new List<PropertyInfo>();

            do
            {
                allProperties.AddRange(type.DeclaredProperties);
                type = type.BaseType.GetTypeInfo();
            }
            while (type != typeof(object).GetTypeInfo());

            return allProperties;
        }

        private static bool IsSupportedSimpleType(Type type)
        {
            return IsNonPointerPrimitive(type) ||
                (IsNullable(type) && IsNonPointerPrimitive(Nullable.GetUnderlyingType(type))) ||
                type == typeof(string) ||
                type.GetTypeInfo().IsEnum;
        }

        private static bool IsNonPointerPrimitive(Type type)
        {
            return type.IsPrimitive && type != typeof(IntPtr) && type != typeof(UIntPtr);
        }

        private static bool IsNullable(Type type)
        {
            return type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
    }
}
