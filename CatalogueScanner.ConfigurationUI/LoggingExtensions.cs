namespace Microsoft.Extensions.Logging;

internal static partial class LoggingExtensions
{
    [LoggerMessage(LogLevel.Warning, "AppServicesAuthenticationInformation.IsAppServicesAadAuthenticationEnabled: {Value}")]
    public static partial void IsAppServicesAadAuthenticationEnabled(this ILogger logger, bool value);

    [LoggerMessage(LogLevel.Error, "HTTP Error: {FriendlyMessage}")]
    public static partial void HttpError(this ILogger logger, Exception exception, string friendlyMessage);

    [LoggerMessage(LogLevel.Warning, "Couldn't find system time zone by provided ID \"{ProvidedTimeZoneId}\" or Windows ID \"{WindowsTimeZoneId}\". Reverting to default time zone \"{DefaultTimeZoneId}\"")]
    public static partial void CouldntFindTimeZone(this ILogger logger, string providedTimeZoneId, string? windowsTimeZoneId, string defaultTimeZoneId);
}
