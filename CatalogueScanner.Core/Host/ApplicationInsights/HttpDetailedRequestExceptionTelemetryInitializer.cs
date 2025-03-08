using CatalogueScanner.Core.Http;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using System.Text.Json;

namespace CatalogueScanner.Core.Host.ApplicationInsights;

public class HttpDetailedRequestExceptionTelemetryInitializer : ITelemetryInitializer
{
    private const string RequestContentPropertyName = $"{nameof(HttpDetailedRequestException)}.{nameof(HttpDetailedRequestException.RequestContent)}";
    private const string ResponseContentPropertyName = $"{nameof(HttpDetailedRequestException)}.{nameof(HttpDetailedRequestException.ResponseContent)}";
    private const string RequestHeadersPropertyName = $"{nameof(HttpDetailedRequestException)}.{nameof(HttpDetailedRequestException.RequestHeaders)}";
    private const string ResponseHeadersPropertyName = $"{nameof(HttpDetailedRequestException)}.{nameof(HttpDetailedRequestException.ResponseHeaders)}";

    public void Initialize(ITelemetry telemetry)
    {
        if (telemetry is ExceptionTelemetry exceptionTelemetry)
        {
            if (exceptionTelemetry.Exception is HttpDetailedRequestException exception)
            {
                InitializeTelemetry(exceptionTelemetry, exception);
            }
            else if (exceptionTelemetry.Exception.InnerException is HttpDetailedRequestException innerException)
            {
                InitializeTelemetry(exceptionTelemetry, innerException);
            }
        }
    }

    private static void InitializeTelemetry(ExceptionTelemetry telemetry, HttpDetailedRequestException exception)
    {
        telemetry.Properties[RequestContentPropertyName] = exception.RequestContent;
        telemetry.Properties[ResponseContentPropertyName] = exception.ResponseContent;

        if (exception.RequestHeaders is not null)
        {
            telemetry.Properties[RequestHeadersPropertyName] = JsonSerializer.Serialize(exception.RequestHeaders, ApplicationInsightsSerializerContext.Default.IReadOnlyDictionaryStringIEnumerableString);
        }

        if (exception.ResponseHeaders is not null)
        {
            telemetry.Properties[ResponseHeadersPropertyName] = JsonSerializer.Serialize(exception.ResponseHeaders, ApplicationInsightsSerializerContext.Default.IReadOnlyDictionaryStringIEnumerableString);
        }
    }
}
