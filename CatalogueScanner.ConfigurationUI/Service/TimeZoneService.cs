using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace CatalogueScanner.ConfigurationUI.Service;

// https://www.meziantou.net/convert-datetime-to-user-s-time-zone-with-server-side-blazor.htm
// https://github.com/SamProf/MatBlazor/issues/663#issuecomment-763528584
public sealed class TimeZoneService(IJSRuntime jsRuntime)
{
    public async ValueTask<DateTimeOffset> GetTimezoneOffset(DateTimeOffset dateTime)
    {
        var offsetInMinutes = await jsRuntime.InvokeAsync<int>("blazorGetTimezoneOffsetForDate", dateTime).ConfigureAwait(false);
        var userOffset = TimeSpan.FromMinutes(-offsetInMinutes);
        return dateTime.ToOffset(userOffset);
    }
}
