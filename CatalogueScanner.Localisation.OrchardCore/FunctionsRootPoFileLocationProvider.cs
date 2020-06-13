using CatalogueScanner.Core.Localisation;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Localization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CatalogueScanner.Localisation.OrchardCore
{
    /// <summary>
    /// Provides localisation files from the root directory of the Functions app.
    /// </summary>
    public class FunctionsRootPoFileLocationProvider : ILocalizationFileLocationProvider
    {
        private readonly IFileProvider fileProvider;
        private readonly string resourcesContainer;

        public FunctionsRootPoFileLocationProvider(IOptionsSnapshot<FunctionsPathOptions> pathOptions, IOptionsSnapshot<LocalizationOptions> localisationOptions)
        {
            #region null checks
            if (pathOptions is null)
            {
                throw new ArgumentNullException(nameof(pathOptions));
            }

            if (localisationOptions is null)
            {
                throw new ArgumentNullException(nameof(localisationOptions));
            }
            #endregion

            fileProvider = new PhysicalFileProvider(pathOptions.Value.RootDirectory);
            resourcesContainer = localisationOptions.Value.ResourcesPath;
        }

        public IEnumerable<IFileInfo> GetLocations(string cultureName)
        {
            return fileProvider.GetDirectoryContents(resourcesContainer) // Localisation/CatalogueScanner.Core, Localisation/CatalogueScanner.SaleFinder, ...
                .Where(f => f.IsDirectory)
                .Select(dir => fileProvider.GetFileInfo(Path.Combine(resourcesContainer, dir.Name, $"{cultureName}.po"))); // Localisation/CatalogueScanner.Core/en.po, Localisation/CatalogueScanner.SaleFinder/en.po, ...
        }
    }
}
