namespace CatalogueScanner.Core.Dto.Api
{
    public record PageInfo(string? ContinuationToken = null, int PageSize = 20);
}
