namespace Microsoft.Extensions.Logging;

internal static partial class LoggingExtensions
{
    [LoggerMessage(LogLevel.Warning, "AppServicesAuthenticationInformation.IsAppServicesAadAuthenticationEnabled: {Value}")]
    public static partial void IsAppServicesAadAuthenticationEnabled(this ILogger logger, bool value);

    [LoggerMessage(LogLevel.Error, "HTTP Error: {FriendlyMessage}")]
    public static partial void HttpError(this ILogger logger, Exception exception, string friendlyMessage);

    [LoggerMessage(LogLevel.Warning, "Couldn't find system time zone by provided ID \"{ProvidedTimeZoneId}\" (Windows ID \"{WindowsTimeZoneId}\"), reverting to default time zone \"{DefaultTimeZoneId}\". Available time zone IDs: {AvailableTimeZoneIds}")]
    public static partial void CouldntFindTimeZone(this ILogger logger, string providedTimeZoneId, string? windowsTimeZoneId, string defaultTimeZoneId, IEnumerable<string> availableTimeZoneIds);

    [LoggerMessage(LogLevel.Debug, "LoadServerData - Cancellation requested, returning empty data")]
    public static partial void LoadServerDataCancellationRequested(this ILogger logger);

    [LoggerMessage(LogLevel.Debug, $"LoadServerData - {nameof(OperationCanceledException)} thrown, returning empty data")]
    public static partial void LoadServerDataOperationCanceledException(this ILogger logger, OperationCanceledException exception);
}
