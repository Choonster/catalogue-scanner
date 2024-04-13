namespace CatalogueScanner.ConfigurationUI.Extensions;

// https://www.meziantou.net/convert-datetime-to-user-s-time-zone-with-server-side-blazor-time-provider.htm
public static class TimeProviderExtensions
{
    public static DateTime ToLocalDateTime(this TimeProvider timeProvider, DateTime dateTime)
    {
        #region null checks
        ArgumentNullException.ThrowIfNull(timeProvider);
        #endregion

        return dateTime.Kind switch
        {
            DateTimeKind.Unspecified => throw new InvalidOperationException("Unable to convert unspecified DateTime to local time"),
            DateTimeKind.Local => dateTime,
            _ => DateTime.SpecifyKind(TimeZoneInfo.ConvertTimeFromUtc(dateTime, timeProvider.LocalTimeZone), DateTimeKind.Local),
        };
    }

    public static DateTime ToLocalDateTime(this TimeProvider timeProvider, DateTimeOffset dateTime)
    {
        #region null checks
        ArgumentNullException.ThrowIfNull(timeProvider);
        #endregion

        var local = TimeZoneInfo.ConvertTimeFromUtc(dateTime.UtcDateTime, timeProvider.LocalTimeZone);
        local = DateTime.SpecifyKind(local, DateTimeKind.Local);
        return local;
    }

    public static DateTime ToUniversalDateTime(this TimeProvider timeProvider, DateTime dateTime)
    {
        #region null checks
        ArgumentNullException.ThrowIfNull(timeProvider);
        #endregion

        return TimeZoneInfo.ConvertTimeToUtc(dateTime, timeProvider.LocalTimeZone);
    }

    public static DateTime? ToUniversalDateTime(this TimeProvider timeProvider, DateTime? dateTime)
    {
        #region null checks
        ArgumentNullException.ThrowIfNull(timeProvider);
        #endregion

        return dateTime.HasValue ? timeProvider.ToUniversalDateTime(dateTime.Value) : null;
    }
}
