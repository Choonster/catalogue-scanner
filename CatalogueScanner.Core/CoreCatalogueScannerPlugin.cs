using CatalogueScanner.Core.Config;
using CatalogueScanner.Core.Host;
using CatalogueScanner.Core.Localisation;
using CatalogueScanner.Core.Options;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Localization;
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

            AddLocalisation(builder);
            AddConfigurationOptions(builder);
        }

        private static void AddLocalisation(ICatalogueScannerHostBuilder builder)
        {
            builder.Services
                .AddMemoryCache()
                .AddPortableObjectLocalization(o => o.ResourcesPath = "Localisation")
                .AddSingleton<ILocalizationFileLocationProvider, FunctionsRootPoFileLocationProvider>()
                .Configure<FunctionsPathOptions>(o =>
                {
                    // https://github.com/Azure/azure-functions-dotnet-extensions/issues/17#issuecomment-499086297
                    var executionContextOptions = builder.Services.BuildServiceProvider().GetService<IOptions<ExecutionContextOptions>>().Value;
                    var appDirectory = executionContextOptions.AppDirectory;
                    o.RootDirectory = appDirectory;
                });

            var localisationCulture = Environment.GetEnvironmentVariable(CoreAppSettingNames.LocalisationCulture);
            if (localisationCulture != null)
            {
                var localisationCultureInfo = CultureInfo.GetCultureInfo(localisationCulture);
                CultureInfo.DefaultThreadCurrentCulture = localisationCultureInfo;
                CultureInfo.DefaultThreadCurrentUICulture = localisationCultureInfo;
            }
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
