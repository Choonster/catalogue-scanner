using MatBlazor;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace CatalogueScanner.ConfigurationUI.Service
{
    public class HttpExceptionHandlingService
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IMatDialogService matDialogService;
        private readonly ILogger<HttpExceptionHandlingService> logger;

        public HttpExceptionHandlingService(IHttpContextAccessor httpContextAccessor, IMatDialogService matDialogService, ILogger<HttpExceptionHandlingService> logger)
        {
            this.matDialogService = matDialogService ?? throw new ArgumentNullException(nameof(matDialogService));
            this.httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task HandleHttpExceptionAsync(HttpRequestException exception, string friendlyErrorMessage)
        {
            #region null checks
            if (exception is null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            if (string.IsNullOrEmpty(friendlyErrorMessage))
            {
                throw new ArgumentException($"'{nameof(friendlyErrorMessage)}' cannot be null or empty.", nameof(friendlyErrorMessage));
            }
            #endregion

            logger.LogError(exception, friendlyErrorMessage);

            var fullMessage = $"{friendlyErrorMessage}: {exception.Message}";

            if (exception.StatusCode == HttpStatusCode.Unauthorized && AppServicesAuthenticationInformation.IsAppServicesAadAuthenticationEnabled)
            {
                fullMessage += "\n\nThis may have happened because your session has expired. Do you want to clear your session and refresh the page?";

                var shouldRefresh = await matDialogService.ConfirmAsync(fullMessage).ConfigureAwait(false);

                if (shouldRefresh)
                {
                    var httpContext = httpContextAccessor.HttpContext;

                    if (httpContext is null)
                    {
                        throw new InvalidOperationException("HttpContext is null");
                    }

                    httpContext.Response.Cookies.Delete("AppServiceAuthSession");
                }
            }
            else
            {
                await matDialogService.AlertAsync(fullMessage).ConfigureAwait(false);
            }
        }
    }
}
