namespace CatalogueScanner.Core.Dto.Api
{
    public class PageInfo
    {
        public string? ContinuationToken { get; set; }
        public int PageSize { get; set; } = 20;
    }
}
