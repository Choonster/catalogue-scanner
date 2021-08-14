﻿using CatalogueScanner.Core.Extensions;
using System;
using System.Collections.Generic;

namespace CatalogueScanner.Core.Utility
{
    /// <summary>
    /// Represents a time of day on a specific day of the week, with time zone information.
    /// </summary>
    public readonly struct TimeOfWeek : IEquatable<TimeOfWeek>
    {
        private const int DaysPerWeek = 7;

        public TimeSpan TimeOfDay { get; }
        public DayOfWeek DayOfWeek { get; }
        public TimeZoneInfo TimeZone { get; }

        public TimeOfWeek(TimeSpan timeOfDay, DayOfWeek dayOfWeek, TimeZoneInfo timeZone)
        {
            TimeOfDay = timeOfDay;
            DayOfWeek = dayOfWeek;
            TimeZone = timeZone;
        }

        public TimeOfWeek(TimeSpan timeOfDay, DayOfWeek dayOfWeek, string timeZoneId) : this(timeOfDay, dayOfWeek, TimeZoneInfo.FindSystemTimeZoneById(timeZoneId))
        { }

        /// <summary>
        /// Returns the closest ocurrence of this time of week prior to or at <paramref name="targetDate"/>.
        /// </summary>
        /// <param name="targetDate">The target date</param>
        /// <returns>The closest occurrence prior to or at the target date</returns>
        public DateTimeOffset GetPreviousDate(DateTimeOffset targetDate)
        {
            // Convert the target date to this instance's time zone
            var result = TimeZoneInfo.ConvertTime(targetDate, TimeZone);

            // Add or subtract the required number of days to reach this instance's day of week
            result = result.AddDays(DayOfWeek - result.DayOfWeek);

            // Set the time of day
            result = result.AtTime(TimeOfDay);

            // If the result is after the target date, subtract a week
            if (result > targetDate)
            {
                result = result.AddDays(-DaysPerWeek);
            }

            return result;
        }

        /// <summary>
        /// Returns the closest ocurrence of this time of week after <paramref name="targetDate"/>.
        /// </summary>
        /// <param name="targetDate">The target date</param>
        /// <returns>The closest occurrence after the target date</returns>
        public DateTimeOffset GetNextDate(DateTimeOffset targetDate)
        {
            // Convert the target date to this instance's time zone
            var result = TimeZoneInfo.ConvertTime(targetDate, TimeZone);

            // Add or subtract the required number of days to reach this instance's day of week
            result = result.AddDays(DayOfWeek - result.DayOfWeek);

            // Set the time of day
            result = result.AtTime(TimeOfDay);

            // If the result is before the target date, add a week
            if (result < targetDate)
            {
                result = result.AddDays(DaysPerWeek);
            }

            return result;
        }

        public override bool Equals(object? obj) => obj is TimeOfWeek other && Equals(other);

        public bool Equals(TimeOfWeek other)
        {
            return TimeOfDay.Equals(other.TimeOfDay) &&
                   DayOfWeek == other.DayOfWeek &&
                   EqualityComparer<TimeZoneInfo>.Default.Equals(TimeZone, other.TimeZone);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(TimeOfDay, DayOfWeek, TimeZone);
        }

        public static bool operator ==(TimeOfWeek left, TimeOfWeek right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TimeOfWeek left, TimeOfWeek right)
        {
            return !(left == right);
        }
    }
}