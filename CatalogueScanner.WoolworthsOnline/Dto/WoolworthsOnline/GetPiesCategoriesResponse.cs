﻿// Originally generated by quicktype (https://quicktype.io/), then manually cleaned up

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace CatalogueScanner.WoolworthsOnline.Dto.WoolworthsOnline;

public partial class GetPiesCategoriesResponse
{
    [JsonInclude]
    public IEnumerable<Category> Categories { get; internal set; } = [];
}

public partial class Category
{
    public string? NodeId { get; set; }
    public string? Description { get; set; }
    public long NodeLevel { get; set; }
    public string? ParentNodeId { get; set; }
    public long DisplayOrder { get; set; }
    public bool IsRestricted { get; set; }
    public long ProductCount { get; set; }
    public bool IsSortEnabled { get; set; }
    public bool IsPaginationEnabled { get; set; }

    [SuppressMessage("Design", "CA1056:URI-like properties should not be strings", Justification = "This property isn't a URI")]
    public string? UrlFriendlyName { get; set; }

    public bool IsSpecial { get; set; }
    public string? RichRelevanceId { get; set; }
    public bool IsBundle { get; set; }

    [JsonInclude]
    public IEnumerable<Category> Children { get; internal set; } = [];
}
