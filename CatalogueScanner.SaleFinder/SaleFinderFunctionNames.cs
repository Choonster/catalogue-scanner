namespace CatalogueScanner.SaleFinder
{
    internal static class SaleFinderFunctionNames
    {
        /// <summary>
        /// The name of the <see cref="Functions.CheckColesCatalogue"/> function.
        /// </summary>
        public const string CheckColesCatalogue = "CheckColesCatalogue";

        /// <summary>
        /// The name of the <see cref="Functions.CheckWoolworthsCatalogue"/> function.
        /// </summary>
        public const string CheckWoolworthsCatalogue = "CheckWoolworthsCatalogue";

        /// <summary>
        /// The name of the <see cref="Functions.CheckIgaCatalogue"/> function.
        /// </summary>
        public const string CheckIgaCatalogue = "CheckIgaCatalogue";

        /// <summary>
        /// The name of the <see cref="Functions.CheckBigWCatalogue"/> function.
        /// </summary>
        public const string CheckBigWCatalogue = "CheckBigWCatalogue";


        /// <summary>
        /// The name of the <see cref="Functions.ScanSaleFinderCatalogue"/> orchestrator function.
        /// </summary>
        public const string ScanSaleFinderCatalogue = "ScanSaleFinderCatalogue";

        /// <summary>
        /// The name of the <see cref="Functions.ScanSaleFinderCatalogue"/> orchestration trigger function.
        /// </summary>
        public const string ScanSaleFinderCatalogue_QueueStart = "ScanSaleFinderCatalogue_QueueStart";


        /// <summary>
        /// The name of the <see cref="Functions.DownloadSaleFinderCatalogue"/> activity function.
        /// </summary>
        public const string DownloadSaleFinderCatalogue = "DownloadSaleFinderCatalogue";

        /// <summary>
        /// The name of the <see cref="Functions.FillSaleFinderItemPrice"/> activity function.
        /// </summary>
        public const string FillSaleFinderItemPrice = "FillSaleFinderItemPrice";
    }
}
