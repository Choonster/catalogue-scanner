using System.Net;

namespace CatalogueScanner.WoolworthsOnline.Dto.FunctionInput;

public record GetWoolworthsOnlineSpecialsPageCountInput(string? CategoryId, CookieCollection Cookies);
