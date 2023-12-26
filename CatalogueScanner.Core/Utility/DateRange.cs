using System;

namespace CatalogueScanner.Core.Utility;

public readonly record struct DateRange(DateTimeOffset StartDate, DateTimeOffset EndDate);
