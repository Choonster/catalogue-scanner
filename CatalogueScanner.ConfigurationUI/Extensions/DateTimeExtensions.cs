namespace CatalogueScanner.ConfigurationUI.Extensions;

public static class DateTimeExtensions
{
    public static DateTime WithTime(this DateTime dateTime, int hour, int minute, int second) =>
        new(dateTime.Year, dateTime.Month, dateTime.Day, hour, minute, second, dateTime.Kind);
}
