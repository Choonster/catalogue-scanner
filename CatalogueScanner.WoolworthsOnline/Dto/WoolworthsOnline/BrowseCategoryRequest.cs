using System;
using System.Diagnostics.CodeAnalysis;

namespace CatalogueScanner.WoolworthsOnline.Dto.WoolworthsOnline;

public class BrowseCategoryRequest
{
    public string? CategoryId { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }

    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Needs to be included in JSON")]
    public Uri Url => new("about:blank");

    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Needs to be included in JSON")]
    public string FormatObject => "{\"name\":\"\"}";
}
