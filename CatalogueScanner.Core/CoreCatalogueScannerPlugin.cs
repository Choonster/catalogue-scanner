﻿using CatalogueScanner.Core.Config;
using CatalogueScanner.Core.Host;
using CatalogueScanner.Core.Localisation;
using CatalogueScanner.Core.Options;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Globalization;

namespace CatalogueScanner.Core
{
    public class CoreCatalogueScannerPlugin : ICatalogueScannerPlugin
    {
        public void Configure(ICatalogueScannerHostBuilder builder)
        {
            #region null checks
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            #endregion

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
            builder.Services.Configure<FunctionsPathOptions>(o =>
            {
                // https://github.com/Azure/azure-functions-dotnet-extensions/issues/17#issuecomment-499086297
                var executionContextOptions = builder.Services.BuildServiceProvider().GetService<IOptionsSnapshot<ExecutionContextOptions>>().Value;
                var appDirectory = executionContextOptions.AppDirectory;
                o.RootDirectory = appDirectory;
            });
        }

        private static void AddConfigurationOptions(ICatalogueScannerHostBuilder builder)
        {
            var coreSection = builder.Configuration.GetSection("Core");

            builder.Services
                .Configure<MatchingOptions>(coreSection.GetSection(MatchingOptions.Matching))
                .Configure<EmailOptions>(coreSection.GetSection(EmailOptions.Email));
        }
    }
}
