﻿using CatalogueScanner.ConfigurationUI.Extensions;
using CatalogueScanner.Core.Dto.Api;
using CatalogueScanner.Core.Dto.Api.Request;
using CatalogueScanner.Core.Functions.Entity;
using CatalogueScanner.Core.Utility;
using MudBlazor;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CatalogueScanner.ConfigurationUI.Components.Pages.Management;

public sealed partial class CatalogueScanStates : IDisposable
{
    private MudTable<CatalogueScanStateDto> table = null!;
    private Dictionary<ScanState, string?> scanStateLabels = [];

    private bool loading;

    private int tablePageIndex;

    private readonly List<CatalogueScanStateDto> loadedScanStates = [];

    private PageInfo pageInfo = new(PageSize: 10);
    private bool isFinalPage;
    private bool hasNoData;

    private MudBlazor.DateRange? lastOperation;

    private int PageSize
    {
        get => pageInfo.PageSize;
        set => pageInfo = pageInfo with { PageSize = value };
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync().ConfigureAwait(true);

        scanStateLabels = new Dictionary<ScanState, string?>
        {
            [ScanState.NotStarted] = S["Not Started"],
            [ScanState.InProgress] = S["In Progress"],
            [ScanState.Completed] = S["Completed"],
            [ScanState.Failed] = S["Failed"],
        };

        Logger.LogInformation("Has local time zone: {Value}", TimeProvider.IsLocalTimeZoneSet);

        if (TimeProvider.IsLocalTimeZoneSet)
        {
            LocalTimeZoneChanged(null, new EventArgs());
        }

        TimeProvider.LocalTimeZoneChanged += LocalTimeZoneChanged;
    }

    public void Dispose()
    {
        TimeProvider.LocalTimeZoneChanged -= LocalTimeZoneChanged;
    }

    private void LocalTimeZoneChanged(object? sender, EventArgs e)
    {
        Logger.LogInformation("Local time zone: {TimeZone}", TimeProvider.LocalTimeZone.DisplayName);

        if (lastOperation is not null)
        {
            return;
        }

        var startOfWeek = new TimeOfWeek(TimeSpan.Zero, DayOfWeek.Monday, TimeProvider.LocalTimeZone);
        var now = TimeProvider.GetLocalNow();

        var lastOperationFrom = TimeProvider.ToLocalDateTime(startOfWeek.GetPreviousDate(now));

        var lastOperationTo = TimeProvider.ToLocalDateTime(startOfWeek.GetNextDate(now).AddDays(-1));

        lastOperation = new(lastOperationFrom, lastOperationTo);

        Logger.LogInformation("Last operation: {From} ({FromKind}) - {To} ({ToKind})", lastOperationFrom, lastOperationFrom.Kind, lastOperationTo, lastOperationTo.Kind);

        _ = InvokeAsync(StateHasChanged);
    }

    private async Task OnDateRangeChanged(MudBlazor.DateRange? lastOperationDateRange)
    {
        try
        {
            loading = true;

            lastOperation = lastOperationDateRange;

            Logger.LogInformation("OnDateRangeChanged - Last operation: {From} ({FromKind}) - {To} ({ToKind})", lastOperation?.Start, lastOperation?.Start?.Kind, lastOperation?.End, lastOperation?.End?.Kind);

            isFinalPage = false;
            tablePageIndex = 0;
            pageInfo = pageInfo with { ContinuationToken = null };

            loadedScanStates.Clear();

            await table.ReloadServerData().ConfigureAwait(true);
        }
        finally
        {
            loading = false;
        }
    }

    private int GetMaxDataIndexForPage(int pageIndex) => (pageIndex + 1) * PageSize - 1;

    private int GetTotalItems()
    {
        // If we've already loaeded the final page of data, return the real count
        if (isFinalPage)
        {
            return loadedScanStates.Count;
        }

        // Otherwise return the equivalent of one more page than the currently loaded data
        var currentPageCount = loadedScanStates.Count / PageSize;
        return (currentPageCount + 1) * PageSize;
    }

    private async Task<TableData<CatalogueScanStateDto>> LoadServerData(TableState tableState)
    {
        tablePageIndex = tableState.Page;
        PageSize = tableState.PageSize;

        // If we haven't loaded the final page of data and the new page would include data that hasn't been loaded yet, load the new page.
        if (!isFinalPage && GetMaxDataIndexForPage(tablePageIndex) >= loadedScanStates.Count)
        {
            try
            {
                // Convert to UTC before serialising to preserve the local time zone
                var lastOperationFrom = TimeProvider.ToUniversalDateTime(lastOperation?.Start);
                var lastOperationTo = TimeProvider.ToUniversalDateTime(lastOperation?.End?.WithTime(23, 59, 59));

                Logger.LogInformation("LoadServerData - Last Operation - {LastOperation}", lastOperation);

                var request = new ListEntityRequest(
                    pageInfo,
                    lastOperationFrom,
                    lastOperationTo
                );                

                var result = await CatalogueScanStateService.ListCatalogueScanStatesAsync(request).ConfigureAwait(true)
                    ?? throw new InvalidOperationException("List Catalogue Scan States request returned no response");

                loadedScanStates.AddRange(result.Entities);

                hasNoData = loadedScanStates.Count == 0;

                pageInfo = pageInfo with { ContinuationToken = result.Page.ContinuationToken };

                isFinalPage = pageInfo.ContinuationToken is null;
            }
            catch (HttpRequestException e)
            {
                await HttpExceptionHandlingService.HandleHttpExceptionAsync(e, "List Catalogue Scan States request failed").ConfigureAwait(false);
            }
        }

        return new()
        {
            Items = loadedScanStates.Skip(tablePageIndex * PageSize)
                                    .Take(PageSize)
                                    .ToList(),
            TotalItems = GetTotalItems(),
        };
    }

    private async Task ResetScanState(CatalogueScanStateDto scanState)
    {
        try
        {
            loading = true;

            scanState = scanState with { ScanState = ScanState.NotStarted };
            await CatalogueScanStateService.UpdateCatalogueScanStateAsync(scanState).ConfigureAwait(true);
        }
        catch (HttpRequestException e)
        {
            await HttpExceptionHandlingService.HandleHttpExceptionAsync(e, "Reset Scan State request failed").ConfigureAwait(false);
        }
        finally
        {
            loading = false;
        }
    }
}
