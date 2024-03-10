using System.Net;

namespace CatalogueScanner.Core.Http;

public class HttpDetailedRequestException : HttpRequestException
{
    public string? RequestContent { get; set; }
    public string? ResponseContent { get; set; }

    public HttpDetailedRequestException()
    {
    }

    public HttpDetailedRequestException(string? message) : base(message)
    {
    }

    public HttpDetailedRequestException(string? message, Exception? inner) : base(message, inner)
    {
    }

    public HttpDetailedRequestException(string? message, Exception? inner, HttpStatusCode? statusCode, string? requestContent, string? responseContent) : base(message, inner, statusCode)
    {
        RequestContent = requestContent;
        ResponseContent = responseContent;
    }
}
