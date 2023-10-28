using Microsoft.Extensions.Logging;

namespace CatalogueScanner.ConfigurationUI.Extensions
{
    public static partial class LoggerMessages
    {
        [LoggerMessage(1, LogLevel.Warning, "AppServicesAuthenticationInformation.IsAppServicesAadAuthenticationEnabled: {Value}")]
        public static partial void IsAppServicesAadAuthenticationEnabled(this ILogger logger, bool value);
    }
}
