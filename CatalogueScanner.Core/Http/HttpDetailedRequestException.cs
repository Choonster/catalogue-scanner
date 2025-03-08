using System.Collections.Immutable;
using System.Net;

namespace CatalogueScanner.Core.Http;

public class HttpDetailedRequestException : HttpRequestException
{
    public string? RequestContent { get; }
    public string? ResponseContent { get; }
    public IReadOnlyDictionary<string, IEnumerable<string>>? RequestHeaders { get; }
    public IReadOnlyDictionary<string, IEnumerable<string>>? ResponseHeaders { get; }

    public HttpDetailedRequestException()
    {
    }

    public HttpDetailedRequestException(string? message) : base(message)
    {
    }

    public HttpDetailedRequestException(string? message, Exception? inner) : base(message, inner)
    {
    }

    public HttpDetailedRequestException(
        string? message,
        Exception? inner,
        HttpStatusCode? statusCode,
        string? requestContent,
        string? responseContent,
        IReadOnlyDictionary<string, IEnumerable<string>>? requestHeaders,
        IReadOnlyDictionary<string, IEnumerable<string>>? responseHeaders
    ) : base(message, inner, statusCode)
    {
        RequestContent = requestContent;
        ResponseContent = responseContent;
        RequestHeaders = requestHeaders?.ToImmutableDictionary();
        ResponseHeaders = responseHeaders?.ToImmutableDictionary();
    }
}
