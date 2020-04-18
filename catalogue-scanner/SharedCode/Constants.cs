namespace CatalogueScanner
{
    internal static class Constants
    {
        public static class FunctionNames
        {
            /// <summary>
            /// The name of the <see cref="CatalogueScanner.CheckColesCatalogue"/> function.
            /// </summary>
            public const string CheckColesCatalogue = "CheckColesCatalogue";


            /// <summary>
            /// The name of the <see cref="CatalogueScanner.ScanSaleFinderCatalogue"/> orchestrator function.
            /// </summary>
            public const string ScanSaleFinderCatalogue = "ScanSaleFinderCatalogue";

            /// <summary>
            /// The name of the <see cref="CatalogueScanner.ScanSaleFinderCatalogue"/> orchestration trigger function.
            /// </summary>
            public const string ScanSaleFinderCatalogue_QueueStart = "ScanSaleFinderCatalogue_QueueStart";


            /// <summary>
            /// The name of the <see cref="CatalogueScanner.DownloadSaleFinderCatalogue"/> activity function.
            /// </summary>
            public const string DownloadSaleFinderCatalogue = "DownloadSaleFinderCatalogue";

            /// <summary>
            /// The name of the <see cref="CatalogueScanner.FilterCatalogueItem"/> activity function.
            /// </summary>
            public const string FilterCatalogueItem = "FilterCatalogueItem";

            /// <summary>
            /// The name of the <see cref="CatalogueScanner.SendCatalogueDigestEmail"/> activity function.
            /// </summary>
            public const string SendCatalogueDigestEmail = "SendCatalogueDigestEmail";
        }

        public static class QueueNames
        {
            public const string SaleFinderCataloguesToScan = "catalogue-scanner-sale-finder-catalogues-to-scan";
        }
    }
}
