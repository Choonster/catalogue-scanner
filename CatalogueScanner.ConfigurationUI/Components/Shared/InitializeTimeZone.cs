using CatalogueScanner.ConfigurationUI.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CatalogueScanner.ConfigurationUI.Components.Shared;

// https://www.meziantou.net/convert-datetime-to-user-s-time-zone-with-server-side-blazor-time-provider.htm
public sealed class InitializeTimeZone : ComponentBase
{
    [Inject] public BrowserTimeProvider TimeProvider { get; set; } = default!;
    [Inject] public IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] public ILogger<InitializeTimeZone> Logger { get; set; } = default!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && !TimeProvider.IsLocalTimeZoneSet)
        {
            try
            {
                var module = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./timezone.js").ConfigureAwait(true);
                await using var _ = module.ConfigureAwait(true);

                Logger.LogInformation("timezone module: {Module}", module);

                var timeZone = await module.InvokeAsync<string>("getBrowserTimeZone").ConfigureAwait(true);

                Logger.LogInformation("getBrowserTimeZone result: {Result}", timeZone);

                TimeProvider.SetBrowserTimeZone(timeZone);
            }
            catch (JSDisconnectedException ex)
            {
                Logger.LogError(ex, "JSDisconnectedException");
            }
        }
    }
}
