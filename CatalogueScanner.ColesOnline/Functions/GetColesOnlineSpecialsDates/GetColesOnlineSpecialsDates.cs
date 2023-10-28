using CatalogueScanner.ColesOnline.Service;
using CatalogueScanner.Core.Utility;
using Microsoft.Azure.Functions.Worker;

namespace CatalogueScanner.ColesOnline.Functions
{
    public static class GetColesOnlineSpecialsDates
    {
        [Function(ColesOnlineFunctionNames.GetColesOnlineSpecialsDates)]
        public static DateRange Run([ActivityTrigger] object? _)
        {
            var specialsResetTime = ColesOnlineService.SpecialsResetTime;

            var now = DateTimeOffset.UtcNow;
            var specialsStartDate = specialsResetTime.GetPreviousDate(now);
            var specialsEndDate = specialsResetTime.GetNextDate(now);

            return new DateRange(specialsStartDate, specialsEndDate);
        }
    }
}
