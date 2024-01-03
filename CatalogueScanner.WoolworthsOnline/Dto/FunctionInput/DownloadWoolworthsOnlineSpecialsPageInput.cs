using System.Net;

namespace CatalogueScanner.WoolworthsOnline.Dto.FunctionInput;

public record DownloadWoolworthsOnlineSpecialsPageInput(string? CategoryId, int PageNumber, CookieCollection Cookies);
