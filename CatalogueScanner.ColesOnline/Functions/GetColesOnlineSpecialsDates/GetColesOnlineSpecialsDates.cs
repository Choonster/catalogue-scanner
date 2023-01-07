using CatalogueScanner.ColesOnline.Service;
using CatalogueScanner.Core.Utility;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace CatalogueScanner.ColesOnline.Functions
{
    public static class GetColesOnlineSpecialsDates
    {
        [FunctionName(ColesOnlineFunctionNames.GetColesOnlineSpecialsDates)]
        public static DateRange Run([ActivityTrigger] IDurableActivityContext context)
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
