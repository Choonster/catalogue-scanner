using CatalogueScanner.ConfigurationUI.Extensions;
using CatalogueScanner.Core.Dto.Api;
using CatalogueScanner.Core.Dto.Api.Request;
using CatalogueScanner.Core.Functions.Entity;
using CatalogueScanner.Core.Utility;
using MatBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CatalogueScanner.ConfigurationUI.Pages.Management
{
    public partial class CatalogueScanStates
    {
        private Dictionary<ScanState, string?> scanStateLabels = new();

        private bool loading;

        private List<CatalogueScanStateDto> tableData = new();
        private int tablePageIndex;

        private readonly List<CatalogueScanStateDto> loadedScanStates = new();

        private PageInfo pageInfo = new(PageSize: 10);
        private bool isFinalPage;
        private bool hasNoData;

        private DateTime? lastOperationFrom;
        private DateTime? lastOperationTo;

        private int PageSize
        {
            get => pageInfo.PageSize;
            set => pageInfo = pageInfo with { PageSize = value };
        }

        private int PaginatorLength
        {
            get
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

            var startOfWeek = new TimeOfWeek(TimeSpan.Zero, DayOfWeek.Monday, TimeZoneInfo.Local);
            var now = DateTimeOffset.Now;

            lastOperationFrom = startOfWeek
                .GetPreviousDate(now)
                .LocalDateTime;

            lastOperationTo = startOfWeek
                .GetNextDate(now)
                .LocalDateTime
                .AddDays(-1);

            await OnPage(new MatPaginatorPageEvent { PageIndex = 0, PageSize = PageSize, Length = 0 }).ConfigureAwait(true);
        }

        private async Task OnFromDateChanged(DateTime? lastOperationFrom)
        {
            this.lastOperationFrom = lastOperationFrom;

            await OnDateRangeChanged().ConfigureAwait(true);
        }

        private async Task OnToDateChanged(DateTime? lastOperationTo)
        {
            this.lastOperationTo = lastOperationTo;

            await OnDateRangeChanged().ConfigureAwait(true);
        }

        private async Task OnDateRangeChanged()
        {
            isFinalPage = false;
            tablePageIndex = 0;
            pageInfo = pageInfo with { ContinuationToken = null };

            await LoadScanStates(resetData: true).ConfigureAwait(true);

            UpdateTableData();
        }

        private async Task OnPage(MatPaginatorPageEvent e)
        {
            tablePageIndex = e.PageIndex;
            PageSize = e.PageSize;

            // If we haven't loaded the final page of data and the new page would include data that hasn't been loaded yet, load the new page.
            if (!isFinalPage && GetMaxDataIndexForPage(tablePageIndex) >= loadedScanStates.Count)
            {
                await LoadScanStates(resetData: false).ConfigureAwait(true);
            }

            UpdateTableData();
        }

        private int GetMaxDataIndexForPage(int pageIndex) => ((pageIndex + 1) * PageSize) - 1;

        private void UpdateTableData()
        {
            tableData = loadedScanStates.Skip(tablePageIndex * PageSize)
                                        .Take(PageSize)
                                        .ToList();
        }

        private async Task LoadScanStates(bool resetData)
        {
            loading = true;

            try
            {
                var request = new ListEntityRequest(
                    pageInfo,
                    lastOperationFrom,
                    lastOperationTo?.WithTime(23, 59, 59)
                );

                var result = await CatalogueScanStateService.ListCatalogueScanStatesAsync(request).ConfigureAwait(true)
                    ?? throw new InvalidOperationException("List Catalogue Scan States request returned no response");

                if (resetData)
                {
                    loadedScanStates.Clear();
                }

                loadedScanStates.AddRange(
                    result.Entities.Select(scanState => scanState with { LastModifiedTime = scanState.LastModifiedTime.ToLocalTime() })
                );

                hasNoData = !loadedScanStates.Any();

                pageInfo = pageInfo with { ContinuationToken = result.Page.ContinuationToken };

                isFinalPage = pageInfo.ContinuationToken is null;
            }
            catch (HttpRequestException e)
            {
                await HttpExceptionHandlingService.HandleHttpExceptionAsync(e, "List Catalogue Scan States request failed").ConfigureAwait(false);
            }

            loading = false;
        }

        private async Task ResetScanState(CatalogueScanStateDto scanState)
        {
            loading = true;

            try
            {
                scanState = scanState with { ScanState = ScanState.NotStarted };
                await CatalogueScanStateService.UpdateCatalogueScanStateAsync(scanState).ConfigureAwait(true);
            }
            catch (HttpRequestException e)
            {
                await HttpExceptionHandlingService.HandleHttpExceptionAsync(e, "Reset Scan State request failed").ConfigureAwait(false);
            }

            loading = false;
        }
    }
}
