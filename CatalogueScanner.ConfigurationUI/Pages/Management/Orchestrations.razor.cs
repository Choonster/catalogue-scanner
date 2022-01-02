using MatBlazor;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CatalogueScanner.ConfigurationUI.Pages.Management
{
    public partial class Orchestrations
    {
        private static readonly Regex httpMethodRegex = new(@"(?<Method>\p{Lu}\p{Ll}+)Uri");

        private bool loading;
        private string? instanceId;
        private IDictionary<string, string>? endpoints;

        public async Task LoadCheckStatusEndpoints()
        {
            loading = true;

            try
            {
                endpoints = await ManagementService.GetCheckStatusEndpointsAsync(instanceId).ConfigureAwait(false);
            }
            catch (HttpRequestException e)
            {
                Logger.LogError(e, "Load Check Status Endpoints request failed");

                await DialogService.AlertAsync($"Load Check Status Endpoints request failed: {e.Message}").ConfigureAwait(true);
            }

            loading = false;
        }

        public async Task CopyPowerShellRequest(string key)
        {
            if (endpoints is null)
            {
                throw new InvalidOperationException("Endpoints haven't been loaded");
            }

            var uri = endpoints[key];

            if (uri is null)
            {
                throw new InvalidOperationException("uri is null");
            }

            var method = new HttpMethod(httpMethodRegex.Match(key).Groups["Method"].Value);

            var script = @$"
$headers =  $headers = @{{ Authorization = 'bearer {TokenProvider.AccessToken}' }}
Invoke-RestMethod -Method {method.Method} -Headers $headers -Uri '{uri}'
            ".Trim();

            await Clipboard.WriteTextAsync(script).ConfigureAwait(false);

            Toaster.Add("Script copied to clipboard", MatToastType.Info);
        }

        public async Task CleanEntityStorage()
        {
            loading = true;

            try
            {
                await ManagementService.CleanEntityStorageAsync().ConfigureAwait(false);
            }
            catch (HttpRequestException e)
            {
                Logger.LogError(e, "Clean Entity Storage request failed");

                await DialogService.AlertAsync($"Clean Entity Storage request failed: {e.Message}").ConfigureAwait(true);
            }

            loading = false;
        }
    }
}
