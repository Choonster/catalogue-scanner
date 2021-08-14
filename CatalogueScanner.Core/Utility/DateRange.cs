using System;

namespace CatalogueScanner.Core.Utility
{
    public readonly struct DateRange : IEquatable<DateRange>
    {
        public DateTimeOffset StartDate { get; }
        public DateTimeOffset EndDate { get; }

        public DateRange(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            StartDate = startDate;
            EndDate = endDate;
        }

        public override bool Equals(object? obj)
        {
            return obj is DateRange range && Equals(range);
        }

        public bool Equals(DateRange other)
        {
            return StartDate.Equals(other.StartDate)
                   && EndDate.Equals(other.EndDate);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(StartDate, EndDate);
        }

        public static bool operator ==(DateRange left, DateRange right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DateRange left, DateRange right)
        {
            return !(left == right);
        }
    }
}
