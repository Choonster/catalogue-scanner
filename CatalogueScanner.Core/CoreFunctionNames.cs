namespace CatalogueScanner.Core
{
    public static class CoreFunctionNames
    {
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
    }
}
