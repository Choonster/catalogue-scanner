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
    public class FunctionsRootPoFileLocationProvider : ILocalizationFileLocationProvider, IDisposable
    {
        private readonly PhysicalFileProvider fileProvider;
        private readonly string resourcesContainer;
        private bool disposedValue;

        public FunctionsRootPoFileLocationProvider(IOptions<FunctionsPathOptions> pathOptions, IOptions<LocalizationOptions> localisationOptions)
        {
            #region null checks
            ArgumentNullException.ThrowIfNull(pathOptions);

            ArgumentNullException.ThrowIfNull(localisationOptions);
            #endregion

            var rootDirectory = pathOptions.Value.RootDirectory
                ?? throw new InvalidOperationException($"{nameof(FunctionsPathOptions)}.{nameof(FunctionsPathOptions.FunctionsPath)} must be configured");

            fileProvider = new PhysicalFileProvider(rootDirectory);
            resourcesContainer = localisationOptions.Value.ResourcesPath;
        }

        public IEnumerable<IFileInfo> GetLocations(string cultureName)
        {
            return fileProvider.GetDirectoryContents(resourcesContainer) // Localisation/CatalogueScanner.Core, Localisation/CatalogueScanner.SaleFinder, ...
                .Where(f => f.IsDirectory)
                .Select(dir => fileProvider.GetFileInfo(Path.Combine(resourcesContainer, dir.Name, $"{cultureName}.po"))); // Localisation/CatalogueScanner.Core/en.po, Localisation/CatalogueScanner.SaleFinder/en.po, ...
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    fileProvider.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
