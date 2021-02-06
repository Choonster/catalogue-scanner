using CatalogueScanner.Core.Dto.Api;
using CatalogueScanner.Core.Dto.Api.Request;
using CatalogueScanner.Core.Dto.Api.Result;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading.Tasks;

namespace CatalogueScanner.ConfigurationUI.Pages.Management
{
    public partial class CatalogueScanStates
    {
        private bool loading;
        private ListEntityResult<CatalogueScanStateDto>? scanStates;
        private string? token;

        public async Task LoadScanStates()
        {
            loading = true;

            token = await TokenAcquisition.GetAccessTokenForUserAsync(new[] { ".default" }).ConfigureAwait(false);

            try
            {
                var scanStates = await CatalogueScanStateService.ListCatalogueScanStatesAsync(new ListEntityRequest()).ConfigureAwait(true);

                this.scanStates = scanStates;
            }
            catch (HttpRequestException e)
            {
                Logger.LogError(e, "List Catalogue Scan States request failed");

                await DialogService.AlertAsync($"List Catalogue Scan States request failed: {e.Message}").ConfigureAwait(true);
            }
            finally
            {
                loading = false;
            }
        }
    }
}
