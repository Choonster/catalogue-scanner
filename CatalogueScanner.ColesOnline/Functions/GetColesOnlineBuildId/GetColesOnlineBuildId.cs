﻿using CatalogueScanner.ColesOnline.Service;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace CatalogueScanner.ColesOnline.Functions
{
    public class GetColesOnlineBuildId
    {
        private readonly ColesOnlineService colesOnlineService;

        public GetColesOnlineBuildId(ColesOnlineService colesOnlineService)
        {
            this.colesOnlineService = colesOnlineService;
        }

        [FunctionName(ColesOnlineFunctionNames.GetColesOnlineBuildId)]
        public async Task<string> Run([ActivityTrigger] IDurableActivityContext context, CancellationToken cancellationToken)
        {
            #region null checks
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            #endregion
                       
            var response = await colesOnlineService.GetBuildId(cancellationToken).ConfigureAwait(false);

            return response;
        }
    }
}