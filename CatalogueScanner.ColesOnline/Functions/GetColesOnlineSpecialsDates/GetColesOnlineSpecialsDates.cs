using CatalogueScanner.ColesOnline.Service;
using CatalogueScanner.Core.Utility;
using Microsoft.Azure.Functions.Worker;

namespace CatalogueScanner.ColesOnline.Functions
{
    public static class GetColesOnlineSpecialsDates
    {
        [Function(ColesOnlineFunctionNames.GetColesOnlineSpecialsDates)]
        public static DateRange Run(
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Required by Azure Functions")]
            [ActivityTrigger]
            object? input
        )
        {
            var specialsResetTime = ColesOnlineService.SpecialsResetTime;

            var now = DateTimeOffset.UtcNow;
            var specialsStartDate = specialsResetTime.GetPreviousDate(now);
            var specialsEndDate = specialsResetTime.GetNextDate(now);

            return new DateRange(specialsStartDate, specialsEndDate);
        }
    }
}
