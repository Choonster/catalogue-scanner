using Azure.Data.AppConfiguration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Options;
using System;
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
        private readonly IConfigurationRefresher configurationRefresher;

        public ConfigurationSaver(IOptionsSnapshot<Options.AzureAppConfigurationOptions> optionsAccessor, ITypedConfiguration<TOptions> typedConfiguration, IConfigurationRefresherProvider configurationRefresherProvider)
        {
            #region null checks
            ArgumentNullException.ThrowIfNull(optionsAccessor);

            ArgumentNullException.ThrowIfNull(configurationRefresherProvider);
            #endregion

            configurationClient = new ConfigurationClient(optionsAccessor.Value.ConnectionString);
            this.typedConfiguration = typedConfiguration ?? throw new ArgumentNullException(nameof(typedConfiguration));
            configurationRefresher = configurationRefresherProvider.Refreshers.First();
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

            await Task.WhenAll(updateTasks).ConfigureAwait(false);
            await Task.WhenAll(deleteTasks).ConfigureAwait(false);

            await RefreshAsync(cancellationToken).ConfigureAwait(false);
        }

        private async Task RefreshAsync(CancellationToken cancellationToken = default)
        {
            await configurationClient.SetConfigurationSettingAsync(ConfigurationConstants.SentinelKey, DateTime.UtcNow.ToString("o"), cancellationToken: cancellationToken).ConfigureAwait(false);
            await configurationRefresher.RefreshAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
