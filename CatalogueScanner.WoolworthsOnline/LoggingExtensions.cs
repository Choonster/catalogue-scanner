using CatalogueScanner.WoolworthsOnline;

namespace Microsoft.Extensions.Logging;

internal static partial class LoggingExtensions
{
    [LoggerMessage(LogLevel.Debug, "Downloading - {CatalogueScanStateKey}")]
    public static partial void Downloading(this ILogger logger, string catalogueScanStateKey);

    [LoggerMessage(LogLevel.Debug, "Updating state - {CatalogueScanStateKey}")]
    public static partial void UpdatingState(this ILogger logger, string catalogueScanStateKey);

    [LoggerMessage(LogLevel.Debug, "Completed - {CatalogueScanStateKey}")]
    public static partial void Completed(this ILogger logger, string catalogueScanStateKey);

    [LoggerMessage(LogLevel.Information, $"Started {WoolworthsOnlineFunctionNames.ScanWoolworthsOnlineSpecials} orchestration with ID = '{{InstanceId}}'.")]
    public static partial void StartedOrchestration(this ILogger logger, string instanceId);
}
