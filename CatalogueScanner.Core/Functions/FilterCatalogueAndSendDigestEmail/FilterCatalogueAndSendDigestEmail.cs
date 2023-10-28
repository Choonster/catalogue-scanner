using CatalogueScanner.Core.Dto.FunctionInput;
using CatalogueScanner.Core.Dto.FunctionResult;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CatalogueScanner.Core.Functions
{
    public static class FilterCatalogueAndSendDigestEmail
    {
        /// <summary>
        /// Sub-orchestrator function that accepts a <see cref="Catalogue"/> and <see cref="EntityInstanceId"/>,
        /// calls <see cref="FilterCatalogueItem"/> for each item and then calls <see cref="SendCatalogueDigestEmail"/>
        /// with the filtered items (if any).
        /// </summary>
        [Function(CoreFunctionNames.FilterCatalogueAndSendDigestEmail)]
        public static async Task RunOrchestrator([OrchestrationTrigger] TaskOrchestrationContext context, FilterCatalogueAndSendDigestEmailInput input)
        {
            #region null checks
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (input is null)
            {
                throw new ArgumentNullException(nameof(input));
            }
            #endregion

            var logger = context.CreateReplaySafeLogger(typeof(FilterCatalogueAndSendDigestEmail));

            var (catalogue, scanStateId) = input;

            if (catalogue is null)
            {
                throw new InvalidOperationException("Catalogue not present");
            }

            #region Filter catalouge items
            context.SetCustomStatus("Filtering");
            logger.LogDebug($"Filtering - {scanStateId.Key}");

            var itemTasks = catalogue.Items
                .Select(item => context.CallActivityAsync<CatalogueItem?>(CoreFunctionNames.FilterCatalogueItem, item))
                .ToList();

            var items = await Task.WhenAll(itemTasks).ConfigureAwait(true);
            #endregion

            #region Send digest email
            context.SetCustomStatus("SendingDigestEmail");
            logger.LogDebug($"Sending digest email - {scanStateId.Key}");

            var filteredItems = items
                .Where(item => item != null)
                .Cast<CatalogueItem>()
                .ToList();

            if (filteredItems.Any())
            {
                var filteredCatalogue = catalogue with { Items = filteredItems };

                await context.CallActivityAsync(CoreFunctionNames.SendCatalogueDigestEmail, filteredCatalogue).ConfigureAwait(true);
            }
            else
            {
                logger.LogInformation($"Catalogue {scanStateId.Key} had no matching items, skipping digest email.");
            }
            #endregion
        }
    }
}
