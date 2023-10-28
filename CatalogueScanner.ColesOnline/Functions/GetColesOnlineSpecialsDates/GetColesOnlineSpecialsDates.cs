using CatalogueScanner.ColesOnline.Service;
using CatalogueScanner.Core.Utility;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;

namespace CatalogueScanner.ColesOnline.Functions
{
    public static class GetColesOnlineSpecialsDates
    {
        // TODO: Might not be able to use TaskActivityContext here
        [Function(ColesOnlineFunctionNames.GetColesOnlineSpecialsDates)]
        public static DateRange Run([ActivityTrigger] TaskActivityContext context)
        {
            #region null checks
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            #endregion

            var specialsResetTime = ColesOnlineService.SpecialsResetTime;

            var now = DateTimeOffset.UtcNow;
            var specialsStartDate = specialsResetTime.GetPreviousDate(now);
            var specialsEndDate = specialsResetTime.GetNextDate(now);

            return new DateRange(specialsStartDate, specialsEndDate);
        }
    }
}
