using CatalogueScanner.ConfigurationUI.Extensions;
using CatalogueScanner.ConfigurationUI.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Globalization;

namespace CatalogueScanner.ConfigurationUI.Components.Shared;

// https://www.meziantou.net/convert-datetime-to-user-s-time-zone-with-server-side-blazor-time-provider.htm
public sealed class LocalTime : ComponentBase, IDisposable
{
    [Inject]
    public BrowserTimeProvider TimeProvider { get; set; } = default!;

    [Parameter]
    public DateTimeOffset? DateTime { get; set; }

    [Parameter]
    public string Format { get; set; } = "G";

    protected override void OnInitialized()
    {
        TimeProvider.LocalTimeZoneChanged += LocalTimeZoneChanged;
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        #region null checks
        ArgumentNullException.ThrowIfNull(builder);
        #endregion

        if (DateTime != null)
        {
            builder.AddContent(0, TimeProvider.ToLocalDateTime(DateTime.Value).ToString(Format, CultureInfo.CurrentCulture));
        }
    }

    public void Dispose()
    {
        TimeProvider.LocalTimeZoneChanged -= LocalTimeZoneChanged;
    }

    private void LocalTimeZoneChanged(object? sender, EventArgs e)
    {
        _ = InvokeAsync(StateHasChanged);
    }
}
