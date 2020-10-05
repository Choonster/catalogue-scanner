using Azure.Data.AppConfiguration;
using CatalogueScanner.Configuration.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CatalogueScanner.Configuration
{
    public class ConfigurationSaver<TOptions> : IConfigurationSaver<TOptions>
    {
        private readonly ConfigurationClient configurationClient;
        private readonly ITypedConfiguration<TOptions> typedConfiguration;

        public ConfigurationSaver(IOptionsSnapshot<AzureAppConfigurationOptions> optionsAccessor, ITypedConfiguration<TOptions> typedConfiguration)
        {
            configurationClient = new ConfigurationClient(optionsAccessor.Value.ConnectionString);
            this.typedConfiguration = typedConfiguration;
        }

        public async Task SaveAsync(TOptions options, CancellationToken cancellationToken = default)
        {
            var newSettings = ReverseConfigurationBinder.GetConfigurationSettings(options, typedConfiguration.Configuration);

            var existingSettings = configurationClient
                .GetConfigurationSettingsAsync(new SettingSelector { KeyFilter = ConfigurationPath.Combine(typedConfiguration.Configuration.Path, "*") }, cancellationToken);

            var existingSettingsDictionary = new Dictionary<string, string>();

            await foreach (var setting in existingSettings)
            {
                existingSettingsDictionary.Add(setting.Key, setting.Value);
            }

            var changedSettingPairs = newSettings
                .Where(newSettingPair =>
                {
                    if (existingSettingsDictionary.TryGetValue(newSettingPair.Key, out var existingValue))
                    {
                        return newSettingPair.Value != existingValue;
                    }

                    return true;
                })
                .ToList();

            var removedSettingPairs = existingSettingsDictionary
                .Where(existingSettingPair => !newSettings.ContainsKey(existingSettingPair.Key))
                .ToList();

            var updateTasks = changedSettingPairs
                .Select(pair => configurationClient.SetConfigurationSettingAsync(pair.Key, pair.Value, cancellationToken: cancellationToken))
                .ToList();

            var deleteTasks = removedSettingPairs
                .Select(pair => configurationClient.DeleteConfigurationSettingAsync(pair.Key))
                .ToList();

            await Task.WhenAll(updateTasks);
            await Task.WhenAll(deleteTasks);
        }
    }
}
