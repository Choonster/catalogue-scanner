﻿using CatalogueScanner.Core.Dto.FunctionInput;
using CatalogueScanner.Core.Dto.FunctionResult;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Entities;
using Microsoft.Extensions.Logging;

namespace CatalogueScanner.Core.Functions;

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
        ArgumentNullException.ThrowIfNull(context);

        ArgumentNullException.ThrowIfNull(input);
        #endregion

        var logger = context.CreateReplaySafeLogger(typeof(FilterCatalogueAndSendDigestEmail));

        var (catalogue, scanStateId) = input;

        if (catalogue is null)
        {
            throw new InvalidOperationException("Catalogue not present");
        }

        #region Filter catalouge items
        context.SetCustomStatus("Filtering");
        logger.Filtering(scanStateId.Key);

        var itemTasks = catalogue.Items
            .Select(item => context.CallActivityAsync<CatalogueItem?>(CoreFunctionNames.FilterCatalogueItem, item))
            .ToList();

        var items = await Task.WhenAll(itemTasks).ConfigureAwait(true);
        #endregion

        #region Send digest email
        context.SetCustomStatus("SendingDigestEmail");
        logger.SendingDigestEmail(scanStateId.Key);

        var filteredItems = items
            .Where(item => item != null)
            .Cast<CatalogueItem>()
            .ToList();

        if (filteredItems.Count > 0)
        {
            var filteredCatalogue = catalogue with { Items = filteredItems };

            await context.CallActivityAsync(CoreFunctionNames.SendCatalogueDigestEmail, filteredCatalogue).ConfigureAwait(true);
        }
        else
        {
            logger.NoMatchingItems(scanStateId.Key);
        }
        #endregion
    }
}
