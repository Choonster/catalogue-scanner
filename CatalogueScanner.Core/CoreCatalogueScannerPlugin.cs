﻿using CatalogueScanner.Configuration;
using CatalogueScanner.Core.Extensions;
using CatalogueScanner.Core.Host;
using CatalogueScanner.Core.Host.ApplicationInsights;
using CatalogueScanner.Core.Localisation;
using CatalogueScanner.Core.MatchRule;
using CatalogueScanner.Core.Options;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Globalization;

namespace CatalogueScanner.Core
{
    public class CoreCatalogueScannerPlugin : ICatalogueScannerPlugin
    {
        private readonly CatalogueItemMatchRuleSerialiser catalogueItemMatchRuleSerialiser = new();

        public void Configure(ICatalogueScannerHostBuilder builder)
        {
            #region null checks
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            #endregion

            builder.Services.AddSingleton<ITelemetryInitializer, HttpDetailedRequestExceptionTelemetryInitializer>();

            SetLocalisationCulture();
            AddFunctionsPathOptions(builder);
            AddConfigurationOptions(builder);
        }

        private static void SetLocalisationCulture()
        {
            var localisationCulture = Environment.GetEnvironmentVariable(CoreAppSettingNames.LocalisationCulture);
            if (localisationCulture != null)
            {
                var localisationCultureInfo = CultureInfo.GetCultureInfo(localisationCulture);
                CultureInfo.DefaultThreadCurrentCulture = localisationCultureInfo;
                CultureInfo.DefaultThreadCurrentUICulture = localisationCultureInfo;
            }
        }

        private static void AddFunctionsPathOptions(ICatalogueScannerHostBuilder builder)
        {
            builder.Services.AddOptions();

            // Manually call AddSingleton for ConfigureNamedOptions to access the IServiceProvider
            builder.Services.AddSingleton<IConfigureOptions<FunctionsPathOptions>, ConfigureNamedOptions<FunctionsPathOptions>>(
                serviceProvider => new ConfigureNamedOptions<FunctionsPathOptions>(Microsoft.Extensions.Options.Options.DefaultName, options =>
                {
                    // https://github.com/Azure/azure-functions-dotnet-extensions/issues/17#issuecomment-499086297
                    var executionContextOptions = serviceProvider.GetRequiredService<IOptionsSnapshot<ExecutionContextOptions>>().Value;
                    var appDirectory = executionContextOptions.AppDirectory;
                    options.RootDirectory = appDirectory;
                })
            );
        }

        private void AddConfigurationOptions(ICatalogueScannerHostBuilder builder)
        {
            var coreSection = builder.Configuration.GetSection("Core");

            var matchingConfig = coreSection.GetSection(MatchingOptions.Matching);

            builder.Services
                .ConfigureOptions<MatchingOptions>(matchingConfig)
                .Configure<MatchingOptions>((options) =>
                {
                    options.Rules.Clear();
                    options.Rules.AddRange(catalogueItemMatchRuleSerialiser.DeserialiseMatchRules(matchingConfig.GetSection("Rules")));
                })
                .ConfigureOptions<EmailOptions>(coreSection.GetSection(EmailOptions.Email));
        }
    }
}
