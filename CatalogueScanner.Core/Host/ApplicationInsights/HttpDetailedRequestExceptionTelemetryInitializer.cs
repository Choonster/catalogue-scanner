using CatalogueScanner.Core.Http;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace CatalogueScanner.Core.Host.ApplicationInsights;

public class HttpDetailedRequestExceptionTelemetryInitializer : ITelemetryInitializer
{
    private const string PropertyName = $"{nameof(HttpDetailedRequestException)}.{nameof(HttpDetailedRequestException.ResponseContent)}";

    public void Initialize(ITelemetry telemetry)
    {
        if (telemetry is ExceptionTelemetry exceptionTelemetry)
        {
            if (exceptionTelemetry.Exception is HttpDetailedRequestException exception)
            {
                exceptionTelemetry.Properties[PropertyName] = exception.ResponseContent;
            }
            else if (exceptionTelemetry.Exception.InnerException is HttpDetailedRequestException innerException)
            {
                exceptionTelemetry.Properties[PropertyName] = innerException.ResponseContent;
            }
        }
    }
}
