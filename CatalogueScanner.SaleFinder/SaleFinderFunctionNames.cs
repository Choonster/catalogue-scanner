using CatalogueScanner.SaleFinder.Functions;

namespace CatalogueScanner.SaleFinder
{
    internal static class SaleFinderFunctionNames
    {
        /// <summary>
        /// The name of the <see cref="CheckColesCatalogue.CheckColesCatalogue"/> function.
        /// </summary>
        public const string CheckColesCatalogue = "CheckColesCatalogue";

        /// <summary>
        /// The name of the <see cref="CheckWoolworthsCatalogue.CheckWoolworthsCatalogue"/> function.
        /// </summary>
        public const string CheckWoolworthsCatalogue = "CheckWoolworthsCatalogue";


        /// <summary>
        /// The name of the <see cref="ScanSaleFinderCatalogue.ScanSaleFinderCatalogue.ScanSaleFinderCatalogue"/> orchestrator function.
        /// </summary>
        public const string ScanSaleFinderCatalogue = "ScanSaleFinderCatalogue";

        /// <summary>
        /// The name of the <see cref="ScanSaleFinderCatalogue.ScanSaleFinderCatalogue.ScanSaleFinderCatalogue"/> orchestration trigger function.
        /// </summary>
        public const string ScanSaleFinderCatalogue_QueueStart = "ScanSaleFinderCatalogue_QueueStart";


        /// <summary>
        /// The name of the <see cref="DownloadSaleFinderCatalogue.DownloadSaleFinderCatalogue"/> activity function.
        /// </summary>
        public const string DownloadSaleFinderCatalogue = "DownloadSaleFinderCatalogue";
    }
}
