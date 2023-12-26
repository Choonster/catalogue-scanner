using CatalogueScanner.ColesOnline.Service;
using CatalogueScanner.Core.Utility;
using Microsoft.Azure.Functions.Worker;

namespace CatalogueScanner.ColesOnline.Functions;

public static class GetColesOnlineSpecialsDates
{
    [Function(ColesOnlineFunctionNames.GetColesOnlineSpecialsDates)]
    public static DateRange Run([ActivityTrigger] DateTime currentUtcDateTime)
    {
        var specialsResetTime = ColesOnlineService.SpecialsResetTime;

        var specialsStartDate = specialsResetTime.GetPreviousDate(currentUtcDateTime);
        var specialsEndDate = specialsResetTime.GetNextDate(currentUtcDateTime);

        return new DateRange(specialsStartDate, specialsEndDate);
    }
}
