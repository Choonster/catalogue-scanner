using CatalogueScanner.Core.Localisation;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Localization;
using System;
using System.Globalization;

namespace CatalogueScanner.Core.Host
{
    public static class HostBuilderExtensions
    {
        public static ICatalogueScannerHostBuilder AddPlugin<T>(this ICatalogueScannerHostBuilder builder) where T : ICatalogueScannerPlugin, new()
        {
            new T().Register(builder);

            return builder;
        }

        public static ICatalogueScannerHostBuilder AddLocalisation(this ICatalogueScannerHostBuilder builder)
        {
            builder.Services.AddMemoryCache();
            builder.Services.AddPortableObjectLocalization(o => o.ResourcesPath = "Localisation");
            builder.Services.AddSingleton<ILocalizationFileLocationProvider, FunctionsRootPoFileLocationProvider>();
            builder.Services.Configure<FunctionsPathOptions>(o =>
            {
                // https://github.com/Azure/azure-functions-dotnet-extensions/issues/17#issuecomment-499086297
                var executionContextOptions = builder.Services.BuildServiceProvider().GetService<IOptions<ExecutionContextOptions>>().Value;
                var appDirectory = executionContextOptions.AppDirectory;
                o.RootDirectory = appDirectory;
            });

            var localisationCulture = Environment.GetEnvironmentVariable("LocalisationCulture");
            if (localisationCulture != null)
            {
                var localisationCultureInfo = CultureInfo.GetCultureInfo(localisationCulture);
                CultureInfo.DefaultThreadCurrentCulture = localisationCultureInfo;
                CultureInfo.DefaultThreadCurrentUICulture = localisationCultureInfo;
            }

            return builder;
        }
    }
}
