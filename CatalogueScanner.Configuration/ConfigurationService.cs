using Azure.Data.AppConfiguration;
using CatalogueScanner.Configuration.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace CatalogueScanner.Configuration
{
    // Adapted from Microsoft.Extensions.Configuration.ConfigurationBinder
    public class ConfigurationService<TOptions> : IConfigurationService<TOptions>
    {
        private readonly ConfigurationClient configurationClient;
        private readonly ITypedConfiguration<TOptions> typedConfiguration;

        public ConfigurationService(IOptionsSnapshot<AzureAppConfigurationOptions> optionsAccessor, ITypedConfiguration<TOptions> typedConfiguration)
        {
            configurationClient = new ConfigurationClient(optionsAccessor.Value.ConnectionString);
            this.typedConfiguration = typedConfiguration;
        }

        public async Task SaveAsync(TOptions options, CancellationToken cancellationToken = default)
        {
            var settings = GetConfigurationSettings(options);

            var tasks = settings
                .Select(pair => configurationClient.SetConfigurationSettingAsync(pair.Key, pair.Value, cancellationToken: cancellationToken))
                .ToList();

            await Task.WhenAll(tasks);
        }

        private Dictionary<string, string> GetConfigurationSettings(TOptions options)
        {
            var binderOptions = new BinderOptions
            {
                BindNonPublicProperties = false,
            };

            var settings = new Dictionary<string, string>();

            FillInstance(typeof(TOptions), options, typedConfiguration.Configuration, binderOptions, settings);

            return settings;
        }

        private static void FillNonScalar(IConfiguration configuration, object instance, BinderOptions options, Dictionary<string, string> settings)
        {
            if (instance != null)
            {
                foreach (PropertyInfo property in GetAllProperties(instance.GetType().GetTypeInfo()))
                {
                    FillProperty(property, instance, configuration, options, settings);
                }
            }
        }

        private static void FillProperty(PropertyInfo property, object instance, IConfiguration config, BinderOptions options, Dictionary<string, string> settings)
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

            FillInstance(property.PropertyType, propertyValue, config.GetSection(property.Name), options, settings);
        }

        private static void FillInstance(Type type, object instance, IConfigurationSection config, BinderOptions options, Dictionary<string, string> settings)
        {
            if (instance is null)
            {
                return;
            }

            var configValue = config?.Value;
            if (configValue != null && TryConvertValue(type, configValue, config.Path, out var convertedValue, out var error))
            {
                if (error != null)
                {
                    throw error;
                }

                settings.Add(config.Path, convertedValue);
            }

            if (config != null && config.GetChildren().Any())
            {
                // See if its a Dictionary
                Type collectionInterface = FindOpenGenericInterface(typeof(IDictionary<,>), type);
                if (collectionInterface != null)
                {
                    FillDictionary(instance, collectionInterface, config, options, settings);
                }
                else if (type.IsArray)
                {
                    FillArray((Array)instance, config, options, settings);
                }
                else
                {
                    // See if its an ICollection
                    collectionInterface = FindOpenGenericInterface(typeof(ICollection<>), type);
                    if (collectionInterface != null)
                    {
                        FillCollection(instance, collectionInterface, config, options, settings);
                    }
                    // Something else
                    else
                    {
                        FillNonScalar(config, instance, options, settings);
                    }
                }
            }

            return;
        }

        private static void FillDictionary(object dictionary, Type dictionaryType, IConfiguration config, BinderOptions options, Dictionary<string, string> settings)
        {
            TypeInfo typeInfo = dictionaryType.GetTypeInfo();

            // IDictionary<K,V> is guaranteed to have exactly two parameters
            Type keyType = typeInfo.GenericTypeArguments[0];
            Type valueType = typeInfo.GenericTypeArguments[1];
            bool keyTypeIsEnum = keyType.GetTypeInfo().IsEnum;

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
                   options: options,
                   settings: settings
                );
            }
        }

        private static void FillCollection(object collection, Type collectionType, IConfiguration config, BinderOptions options, Dictionary<string, string> settings)
        {
            if (collection is null)
            {
                return;
            }

            TypeInfo typeInfo = collectionType.GetTypeInfo();

            // ICollection<T> is guaranteed to have exactly one parameter
            Type itemType = typeInfo.GenericTypeArguments[0];

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
                        options: options,
                        settings: settings
                    );
                }
                catch
                {
                }

                index++;
            }
        }

        private static void FillArray(Array source, IConfiguration config, BinderOptions options, Dictionary<string, string> settings)
        {
            if (source is null)
            {
                return;
            }

            Type elementType = source.GetType().GetElementType();

            for (int i = 0; i < source.Length; i++)
            {
                try
                {
                    FillInstance(
                        type: elementType,
                        instance: source.GetValue(i),
                        config: config.GetSection(i.ToString()),
                        options: options,
                        settings: settings
                    );
                }
                catch
                {
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

            if (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                if (value is null)
                {
                    return true;
                }
                return TryConvertValue(Nullable.GetUnderlyingType(type), value, path, out result, out error);
            }

            TypeConverter converter = TypeDescriptor.GetConverter(type);
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

        private static Type FindOpenGenericInterface(Type expected, Type actual)
        {
            TypeInfo actualTypeInfo = actual.GetTypeInfo();
            if (actualTypeInfo.IsGenericType &&
                actual.GetGenericTypeDefinition() == expected)
            {
                return actual;
            }

            IEnumerable<Type> interfaces = actualTypeInfo.ImplementedInterfaces;
            foreach (Type interfaceType in interfaces)
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
    }
}
