﻿namespace CatalogueScanner.Core.Extensions;

public static class DateExtensions
{
    public static DateTimeOffset AtTime(this DateTimeOffset date, TimeSpan timeOfDay) =>
        new(date.Year, date.Month, date.Day, timeOfDay.Hours, timeOfDay.Minutes, timeOfDay.Seconds, timeOfDay.Milliseconds, date.Offset);
}
