using System;

namespace CatalogueScanner.Core.Dto.Api.Request
{
    public record ListEntityRequest(PageInfo Page, DateTimeOffset? LastModifiedFrom, DateTimeOffset? LastModifiedTo);
}
