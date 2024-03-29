﻿using CatalogueScanner.Core.Host;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.Localization;

namespace CatalogueScanner.Localisation.OrchardCore;

public class OrchardCoreLocalisationCatalogueScannerPlugin : ICatalogueScannerPlugin
{
    public void Configure(ICatalogueScannerHostBuilder builder)
    {
        #region null checks
        ArgumentNullException.ThrowIfNull(builder);
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
