using CatalogueScanner.Core.Http;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace CatalogueScanner.Core.Host.ApplicationInsights;

public class HttpDetailedRequestExceptionTelemetryInitializer : ITelemetryInitializer
{
    private const string RequestContentPropertyName = $"{nameof(HttpDetailedRequestException)}.{nameof(HttpDetailedRequestException.RequestContent)}";
    private const string ResponseContentPropertyName = $"{nameof(HttpDetailedRequestException)}.{nameof(HttpDetailedRequestException.ResponseContent)}";

    public void Initialize(ITelemetry telemetry)
    {
        if (telemetry is ExceptionTelemetry exceptionTelemetry)
        {
            if (exceptionTelemetry.Exception is HttpDetailedRequestException exception)
            {
                exceptionTelemetry.Properties[RequestContentPropertyName] = exception.RequestContent;
                exceptionTelemetry.Properties[ResponseContentPropertyName] = exception.ResponseContent;
            }
            else if (exceptionTelemetry.Exception.InnerException is HttpDetailedRequestException innerException)
            {
                exceptionTelemetry.Properties[RequestContentPropertyName] = innerException.RequestContent;
                exceptionTelemetry.Properties[ResponseContentPropertyName] = innerException.ResponseContent;
            }
        }
    }
}
