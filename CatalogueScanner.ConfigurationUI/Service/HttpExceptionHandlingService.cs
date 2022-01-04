using MatBlazor;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Microsoft.JSInterop;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace CatalogueScanner.ConfigurationUI.Service
{
    public class HttpExceptionHandlingService
    {
        private readonly IJSRuntime jsRuntime;
        private readonly IMatDialogService matDialogService;
        private readonly ILogger<HttpExceptionHandlingService> logger;

        public HttpExceptionHandlingService(IJSRuntime jsRuntime, IMatDialogService matDialogService, ILogger<HttpExceptionHandlingService> logger)
        {
            this.jsRuntime = jsRuntime;
            this.matDialogService = matDialogService;
            this.logger = logger;
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

            if (true || exception.StatusCode == HttpStatusCode.Unauthorized && AppServicesAuthenticationInformation.IsAppServicesAadAuthenticationEnabled)
            {
                fullMessage += "\n\nThis may have happened because your session has expired. Do you want to clear your session and refresh the page?";

                var shouldRefresh = await matDialogService.ConfirmAsync(fullMessage).ConfigureAwait(false);

                if (shouldRefresh)
                {
                    await jsRuntime.InvokeVoidAsync("blazorClearAppServicesAuthenticationSession").ConfigureAwait(false);                    
                }
            }
            else
            {
                await matDialogService.AlertAsync(fullMessage).ConfigureAwait(false);
            }
        }
    }
}
