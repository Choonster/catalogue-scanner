using CatalogueScanner.Core.Functions.Entity;

namespace Microsoft.Extensions.Logging
{
    internal static partial class LoggingExtensions
    {
        [LoggerMessage(LogLevel.Debug, "Checking state - {CatalogueScanStateKey}")]
        public static partial void CheckingState(this ILogger logger, string catalogueScanStateKey);

        [LoggerMessage(LogLevel.Information, "Catalogue {CatalogueScanStateKey} already in state {ScanState}, skipping scan.")]
        public static partial void SkippingScan(this ILogger logger, string catalogueScanStateKey, ScanState scanState);

        [LoggerMessage(LogLevel.Debug, "Filtering - {CatalogueScanStateKey}")]
        public static partial void Filtering(this ILogger logger, string catalogueScanStateKey);

        [LoggerMessage(LogLevel.Debug, "Sending digest email - {CatalogueScanStateKey}")]
        public static partial void SendingDigestEmail(this ILogger logger, string catalogueScanStateKey);

        [LoggerMessage(LogLevel.Information, "Catalogue {CatalogueScanStateKey} had no matching items, skipping digest email.")]
        public static partial void NoMatchingItems(this ILogger logger, string catalogueScanStateKey);
    }
}
