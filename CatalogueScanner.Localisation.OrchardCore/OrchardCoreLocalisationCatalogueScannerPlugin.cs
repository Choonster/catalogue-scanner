using CatalogueScanner.Core.Host;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.Localization;
using System;

namespace CatalogueScanner.Localisation.OrchardCore
{
    public class OrchardCoreLocalisationCatalogueScannerPlugin : ICatalogueScannerPlugin
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
        }

        private static void AddLocalisation(ICatalogueScannerHostBuilder builder)
        {
            builder.Services
                .AddMemoryCache()
                .AddPortableObjectLocalization(o => o.ResourcesPath = "Localisation");

            builder.Services.TryAddSingleton<ILocalizationFileLocationProvider, FunctionsRootPoFileLocationProvider>();
            builder.Services.TryAddTransient(typeof(Core.Localisation.IPluralStringLocalizer<>), typeof(PluralStringLocalizer<>));
        }
    }
}
