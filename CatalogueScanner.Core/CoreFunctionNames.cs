namespace CatalogueScanner.Core
{
    public static class CoreFunctionNames
    {
        /// <summary>
        /// The name of the <see cref="Functions.CheckAndUpdateScanState"/> sub-orchestrator function.
        /// </summary>
        public const string CheckAndUpdateScanState = "CheckAndUpdateScanState";

        /// <summary>
        /// The name of the <see cref="Functions.FilterCatalogueAndSendDigestEmail"/> sub-orchestrator function.
        /// </summary>
        public const string FilterCatalogueAndSendDigestEmail = "FilterCatalogueAndSendDigestEmail";

        /// <summary>
        /// The name of the <see cref="Functions.FilterCatalogueItem"/> activity function.
        /// </summary>
        public const string FilterCatalogueItem = "FilterCatalogueItem";

        /// <summary>
        /// The name of the <see cref="Functions.SendCatalogueDigestEmail"/> activity function.
        /// </summary>
        public const string SendCatalogueDigestEmail = "SendCatalogueDigestEmail";

        /// <summary>
        /// The name of the <see cref="Functions.Api.ListCatalogueScanStates"/> function
        /// </summary>
        public const string ListCatalogueScanStates = "ListCatalogueScanStates";

        /// <summary>
        /// The name of the <see cref="Functions.Api.UpdateCatalogueScanState"/> function.
        /// </summary>
        public const string UpdateCatalogueScanState = "UpdateCatalogueScanState";

        /// <summary>
        /// The name of the <see cref="Functions.Api.Management.CleanEntityStorage"/> function.
        /// </summary>
        public const string CleanEntityStorage = "CleanEntityStorage";

        /// <summary>
        /// The name of the <see cref="Functions.Api.Management.GetCheckStatusEndpoints"/> function.
        /// </summary>
        public const string GetCheckStatusEndpoints = "GetCheckStatusEndpoints";
    }
}
