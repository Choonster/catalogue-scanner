using CatalogueScanner.SaleFinder;

namespace Microsoft.Extensions.Logging;

internal static partial class LoggingExtensions
{
    [LoggerMessage(LogLevel.Information, "Found sale IDs: {SaleIds}")]
    public static partial void FoundSaleIds(this ILogger logger, IEnumerable<int> saleIds);

    [LoggerMessage(LogLevel.Information, "Successfully downloaded and parsed catalogue with {NumPages} page(s)")]
    public static partial void SuccessfullyDownloadedAndParsedCatalogue(this ILogger logger, int numPages);

    [LoggerMessage(LogLevel.Error, "Item ID {ItemId}: Unknown format for span.sf-saleoptiondesc: \"{SaleOptionDescText}\"")]
    public static partial void UnknownFormat(this ILogger logger, string? itemId, string saleOptionDescText);

    [LoggerMessage(LogLevel.Debug, "Downloading - {CatalogueScanStateKey}")]
    public static partial void Downloading(this ILogger logger, string catalogueScanStateKey);

    [LoggerMessage(LogLevel.Debug, "Filling Prices - {CatalogueScanStateKey}")]
    public static partial void FillingPrices(this ILogger logger, string catalogueScanStateKey);

    [LoggerMessage(LogLevel.Debug, "Updating state - {CatalogueScanStateKey}")]
    public static partial void UpdatingState(this ILogger logger, string catalogueScanStateKey);

    [LoggerMessage(LogLevel.Debug, "Completed - {CatalogueScanStateKey}")]
    public static partial void Completed(this ILogger logger, string catalogueScanStateKey);

    [LoggerMessage(LogLevel.Information, $"Started {SaleFinderFunctionNames.ScanSaleFinderCatalogue} orchestration with ID = '{{InstanceId}}'.")]
    public static partial void StartedOrchestration(this ILogger logger, string instanceId);
}
