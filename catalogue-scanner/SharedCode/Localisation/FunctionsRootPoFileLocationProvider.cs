using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Localization;
using System;
using System.Collections.Generic;
using System.IO;

namespace CatalogueScanner.Localisation
{
    /// <summary>
    /// Provides localisation files from the root directory of the Functions app.
    /// </summary>
    public class FunctionsRootPoFileLocationProvider : ILocalizationFileLocationProvider
    {
        private readonly IFileProvider fileProvider;
        private readonly string resourcesContainer;

        public FunctionsRootPoFileLocationProvider(IOptions<FunctionsPathOptions> pathOptions, IOptions<LocalizationOptions> localisationOptions)
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
            yield return fileProvider.GetFileInfo(Path.Combine(resourcesContainer, cultureName + ".po"));
        }
    }
}
