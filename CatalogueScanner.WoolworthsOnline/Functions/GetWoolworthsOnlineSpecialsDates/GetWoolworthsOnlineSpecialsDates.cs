using CatalogueScanner.Core.Utility;
using CatalogueScanner.WoolworthsOnline.Service;
using Microsoft.Azure.Functions.Worker;
using System;

namespace CatalogueScanner.WoolworthsOnline.Functions;

public static class GetWoolworthsOnlineSpecialsDates
{
    [Function(WoolworthsOnlineFunctionNames.GetWoolworthsOnlineSpecialsDates)]
    public static DateRange Run(
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Required by Azure Functions")]
        [ActivityTrigger]
        object? input
    )
    {
        var specialsResetTime = WoolworthsOnlineService.SpecialsResetTime;

        var now = DateTimeOffset.UtcNow;
        var specialsStartDate = specialsResetTime.GetPreviousDate(now);
        var specialsEndDate = specialsResetTime.GetNextDate(now);

        return new DateRange(specialsStartDate, specialsEndDate);
    }
}
