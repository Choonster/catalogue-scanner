namespace CatalogueScanner.SaleFinder.Dto.SaleFinder;

/// <summary>
/// Returned from the SaleFinder Catalogue View request:
/// https://embed.salefinder.com.au/catalogues/view/{storeId}/?locationId={locationId}
/// </summary>
public partial class CatalogueViewResponse
{
    public string? Content { get; set; }
    public string? Breadcrumb { get; set; }
    public string? Region { get; set; }
    public string? Redirect { get; set; }
}
