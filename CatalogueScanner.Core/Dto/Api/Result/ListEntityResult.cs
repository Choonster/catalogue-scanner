using System.Collections.Generic;

namespace CatalogueScanner.Core.Dto.Api.Result
{
    public record ListEntityResult<T>(IEnumerable<T> Entities, PageInfo Page);
}
