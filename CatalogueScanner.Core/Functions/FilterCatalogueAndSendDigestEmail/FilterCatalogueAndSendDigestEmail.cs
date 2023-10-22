using CatalogueScanner.Core.Dto.FunctionInput;
using CatalogueScanner.Core.Dto.FunctionResult;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CatalogueScanner.Core.Functions
{
    public static class FilterCatalogueAndSendDigestEmail
    {
        /// <summary>
        /// Sub-orchestrator function that accepts a <see cref="Catalogue"/> and <see cref="EntityId"/>,
        /// calls <see cref="FilterCatalogueItem"/> for each item and then calls <see cref="SendCatalogueDigestEmail"/>
        /// with the filtered items (if any).
        /// </summary>
        [FunctionName(CoreFunctionNames.FilterCatalogueAndSendDigestEmail)]
        public static async Task RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            #region null checks
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (log is null)
            {
                throw new ArgumentNullException(nameof(log));
            }
            #endregion

            var input = context.GetInput<FilterCatalogueAndSendDigestEmailInput>()
                ?? throw new InvalidOperationException("Orchestrator function input not present");

            var (catalogue, scanStateId) = input;

            if (catalogue is null)
            {
                throw new InvalidOperationException("Catalogue not present");
            }

            #region Filter catalouge items
            context.SetCustomStatus("Filtering");
            log.LogDebug($"Filtering - {scanStateId.EntityKey}");

            var itemTasks = catalogue.Items
                .Select(item => context.CallActivityAsync<CatalogueItem?>(CoreFunctionNames.FilterCatalogueItem, item))
                .ToList();

            await Task.WhenAll(itemTasks).ConfigureAwait(true);
            #endregion

            #region Send digest email
            context.SetCustomStatus("SendingDigestEmail");
            log.LogDebug($"Sending digest email - {scanStateId.EntityKey}");

            var filteredItems = itemTasks
                .Where(task => task.Result != null)
                .Select(task => task.Result!)
                .ToList();

            if (filteredItems.Any())
            {
                var filteredCatalogue = catalogue with { Items = filteredItems };

                await context.CallActivityAsync(CoreFunctionNames.SendCatalogueDigestEmail, filteredCatalogue).ConfigureAwait(true);
            }
            else
            {
                log.LogInformation($"Catalogue {scanStateId.EntityKey} had no matching items, skipping digest email.");
            }
            #endregion
        }
    }
}
