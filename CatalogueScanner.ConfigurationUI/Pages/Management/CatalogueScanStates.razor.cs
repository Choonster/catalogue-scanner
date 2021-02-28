using CatalogueScanner.ConfigurationUI.ViewModel;
using CatalogueScanner.Core.Dto.Api;
using CatalogueScanner.Core.Dto.Api.Request;
using CatalogueScanner.Core.Functions.Entity;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace CatalogueScanner.ConfigurationUI.Pages.Management
{
    public partial class CatalogueScanStates
    {
        private Dictionary<ScanState, string?> scanStateLabels = new Dictionary<ScanState, string?>();

        private bool loading;
        private readonly List<CatalogueScanStateDto> scanStates = new List<CatalogueScanStateDto>();
        private PageInfo pageInfo = new PageInfo { PageSize = 20 };
        private bool isFinalPage;        

        protected override void OnInitialized()
        {
            base.OnInitialized();

            scanStateLabels = new Dictionary<ScanState, string?>
            {
                [ScanState.NotStarted] = S["Not Started"],
                [ScanState.InProgress] = S["In Progress"],
                [ScanState.Completed] = S["Completed"],
            };
        }

        private async Task LoadScanStates()
        {
            loading = true;

            try
            {
                var result = await CatalogueScanStateService.ListCatalogueScanStatesAsync(new ListEntityRequest { Page = pageInfo }).ConfigureAwait(true);

                scanStates.AddRange(result.Entities);

                pageInfo = result.Page;

                // TODO: This may not be the right way to check for the final page
                isFinalPage = result.Page.ContinuationToken is null;
            }
            catch (HttpRequestException e)
            {
                Logger.LogError(e, "List Catalogue Scan States request failed");

                await DialogService.AlertAsync($"List Catalogue Scan States request failed: {e.Message}").ConfigureAwait(true);
            }

            loading = false;
        }

        private async Task ResetScanState(CatalogueScanStateDto scanState)
        {
            await CatalogueScanStateService.UpdateCatalogueScanStateAsync(scanState).ConfigureAwait(true);
        }
    }
}
