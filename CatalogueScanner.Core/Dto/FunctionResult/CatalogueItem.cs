namespace CatalogueScanner.Core.Dto.FunctionResult;

public record CatalogueItem(string? Id, string? Name, string? Sku, Uri? Uri, decimal? Price, long? MultiBuyQuantity, decimal? MultiBuyTotalPrice);
