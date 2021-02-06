using System;

namespace CatalogueScanner.ConfigurationUI.Options
{
    public class CatalogueScannerApiOptions
    {
        public const string CatalogueScannerApi = "CatalogueScannerApi";

        public Uri? BaseAddress { get; set; }
    }
}
