namespace CatalogueScanner.SaleFinder
{
    class SaleFinderConstants
    {
        public static class FunctionNames
        {
            /// <summary>
            /// The name of the <see cref="Functions.CheckColesCatalogue.CheckColesCatalogue"/> function.
            /// </summary>
            public const string CheckColesCatalogue = "CheckColesCatalogue";

            /// <summary>
            /// The name of the <see cref="Functions.CheckWoolworthsCatalogue.CheckWoolworthsCatalogue"/> function.
            /// </summary>
            public const string CheckWoolworthsCatalogue = "CheckWoolworthsCatalogue";


            /// <summary>
            /// The name of the <see cref="Functions.ScanSaleFinderCatalogue.ScanSaleFinderCatalogue"/> orchestrator function.
            /// </summary>
            public const string ScanSaleFinderCatalogue = "ScanSaleFinderCatalogue";

            /// <summary>
            /// The name of the <see cref="Functions.ScanSaleFinderCatalogue.ScanSaleFinderCatalogue"/> orchestration trigger function.
            /// </summary>
            public const string ScanSaleFinderCatalogue_QueueStart = "ScanSaleFinderCatalogue_QueueStart";


            /// <summary>
            /// The name of the <see cref="CatalogueScanner.DownloadSaleFinderCatalogue"/> activity function.
            /// </summary>
            public const string DownloadSaleFinderCatalogue = "DownloadSaleFinderCatalogue";
        }

        public static class QueueNames
        {
            public const string SaleFinderCataloguesToScan = "catalogue-scanner-sale-finder-catalogues-to-scan";
        }

        public static class AppSettingNames
        {
            /// <summary>
            /// The CRON expression used for the timer triggers of the Check[Store]Catalogue functions.
            /// </summary>
            public const string CheckCatalogueFunctionCronExpression = "CheckCatalogueFunctionCronExpression";
        }
    }
}
