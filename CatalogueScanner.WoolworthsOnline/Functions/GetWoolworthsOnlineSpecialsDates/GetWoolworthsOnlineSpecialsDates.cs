using CatalogueScanner.Core.Utility;
using CatalogueScanner.WoolworthsOnline.Service;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using System;

namespace CatalogueScanner.WoolworthsOnline.Functions
{
    public static class GetWoolworthsOnlineSpecialsDates
    {
        // TODO: Might not be able to use TaskActivityContext here
        [Function(WoolworthsOnlineFunctionNames.GetWoolworthsOnlineSpecialsDates)]
        public static DateRange Run([ActivityTrigger] TaskActivityContext context)
        {
            #region null checks
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            #endregion

            var specialsResetTime = WoolworthsOnlineService.SpecialsResetTime;

            var now = DateTimeOffset.UtcNow;
            var specialsStartDate = specialsResetTime.GetPreviousDate(now);
            var specialsEndDate = specialsResetTime.GetNextDate(now);

            return new DateRange(specialsStartDate, specialsEndDate);
        }
    }
}
