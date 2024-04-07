namespace CatalogueScanner.ConfigurationUI.Service;

// https://www.meziantou.net/convert-datetime-to-user-s-time-zone-with-server-side-blazor-time-provider.htm
public sealed class BrowserTimeProvider : TimeProvider
{
    private readonly ILogger<BrowserTimeProvider> logger;

    public BrowserTimeProvider(ILogger<BrowserTimeProvider> logger)
    {
        this.logger = logger;
    }

    private TimeZoneInfo? browserLocalTimeZone;

    public event EventHandler? LocalTimeZoneChanged;

    public override TimeZoneInfo LocalTimeZone
        => browserLocalTimeZone ?? base.LocalTimeZone;

    internal bool IsLocalTimeZoneSet => browserLocalTimeZone != null;

    public void SetBrowserTimeZone(string timeZone)
    {
        logger.LogInformation("SetBrowserTimeZone: {TimeZone}", timeZone);

        if (!TimeZoneInfo.TryFindSystemTimeZoneById(timeZone, out var timeZoneInfo))
        {
            logger.LogInformation("Couldn't find Time Zone by ID. Available Time Zones: {AvailableTimeZones}", TimeZoneInfo.GetSystemTimeZones());
            timeZoneInfo = null;
        }

        if (timeZoneInfo != LocalTimeZone)
        {
            browserLocalTimeZone = timeZoneInfo;
            LocalTimeZoneChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
