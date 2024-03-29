﻿using CatalogueScanner.Configuration;
using CatalogueScanner.Core.Extensions;
using CatalogueScanner.Core.Host;
using CatalogueScanner.Core.Host.ApplicationInsights;
using CatalogueScanner.Core.Localisation;
using CatalogueScanner.Core.MatchRule;
using CatalogueScanner.Core.Options;
using CatalogueScanner.Core.Serialisation;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text.Json;

namespace CatalogueScanner.Core;

public class CoreCatalogueScannerPlugin : ICatalogueScannerPlugin
{
    public void Configure(ICatalogueScannerHostBuilder builder)
    {
        #region null checks
        ArgumentNullException.ThrowIfNull(builder);
        #endregion

        builder.Services.AddSingleton<ITelemetryInitializer, HttpDetailedRequestExceptionTelemetryInitializer>();

        builder.Services.Configure<JsonSerializerOptions>(options =>
        {
            options.Converters.Add(new SendGridMessageConverter());
            options.Converters.Add(new PersonalizationConverter());
            options.Converters.Add(new AttachmentConverter());
        });

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
                // https://stackoverflow.com/a/68092901
                var localRoot = Environment.GetEnvironmentVariable("AzureWebJobsScriptRoot");
                var azureRoot = $"{Environment.GetEnvironmentVariable("HOME")}/site/wwwroot";
                var actualRoot = localRoot ?? azureRoot;

                options.RootDirectory = actualRoot;
            })
        );
    }

    private static void AddConfigurationOptions(ICatalogueScannerHostBuilder builder)
    {
        var coreSection = builder.Configuration.GetSection("Core");

        var matchingConfig = coreSection.GetSection(MatchingOptions.Matching);

        builder.Services
            .ConfigureOptions<MatchingOptions>(matchingConfig)
            .Configure<MatchingOptions>((options) =>
            {
                options.Rules.Clear();
                options.Rules.AddRange(CatalogueItemMatchRuleSerialiser.DeserialiseMatchRules(matchingConfig.GetSection("Rules")));
            })
            .ConfigureOptions<EmailOptions>(coreSection.GetSection(EmailOptions.Email));
    }
}
