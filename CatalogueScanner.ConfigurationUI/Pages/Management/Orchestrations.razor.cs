using MatBlazor;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CatalogueScanner.ConfigurationUI.Pages.Management;

public partial class Orchestrations
{
    [GeneratedRegex(@"(?<Method>\p{Lu}\p{Ll}+)Uri")]
    private static partial Regex HttpMethodRegex();

    private bool loading;
    private string? instanceId;
    private IDictionary<string, string>? endpoints;

    public async Task LoadCheckStatusEndpoints()
    {
        loading = true;

        try
        {
            endpoints = await ManagementService.GetCheckStatusEndpointsAsync(instanceId).ConfigureAwait(false)
                ?? throw new InvalidOperationException("Load Check Status Endpoints request returned no response");
        }
        catch (HttpRequestException e)
        {
            await HttpExceptionHandlingService.HandleHttpExceptionAsync(e, "Load Check Status Endpoints request failed").ConfigureAwait(false);
        }

        loading = false;
    }

    public async Task CopyPowerShellRequest(string key)
    {
        if (endpoints is null)
        {
            throw new InvalidOperationException("Endpoints haven't been loaded");
        }

        var uri = endpoints[key] ?? throw new InvalidOperationException("uri is null");

        var method = new HttpMethod(HttpMethodRegex().Match(key).Groups["Method"].Value);

        var script = @$"
$headers = @{{ Authorization = 'bearer {TokenProvider.AccessToken}' }}
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
            await HttpExceptionHandlingService.HandleHttpExceptionAsync(e, "Clean Entity Storage request failed").ConfigureAwait(false);
        }

        loading = false;
    }
}
