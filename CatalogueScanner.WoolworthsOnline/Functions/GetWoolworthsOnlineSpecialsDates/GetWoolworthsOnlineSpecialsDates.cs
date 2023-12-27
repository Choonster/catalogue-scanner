using CatalogueScanner.Core.Utility;
using CatalogueScanner.WoolworthsOnline.Service;
using Microsoft.Azure.Functions.Worker;

namespace CatalogueScanner.WoolworthsOnline.Functions;

public static class GetWoolworthsOnlineSpecialsDates
{
    [Function(WoolworthsOnlineFunctionNames.GetWoolworthsOnlineSpecialsDates)]
    public static DateRange Run([ActivityTrigger] DateTime currentUtcDateTime)
    {
        var specialsResetTime = WoolworthsOnlineService.SpecialsResetTime;

        var specialsStartDate = specialsResetTime.GetPreviousDate(currentUtcDateTime);
        var specialsEndDate = specialsResetTime.GetNextDate(currentUtcDateTime);

        return new DateRange(specialsStartDate, specialsEndDate);
    }
}
