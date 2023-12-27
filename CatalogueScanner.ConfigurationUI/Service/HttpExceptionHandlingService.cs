using MatBlazor;
using Microsoft.Identity.Web;
using Microsoft.JSInterop;
using System.Net;

namespace CatalogueScanner.ConfigurationUI.Service;

public class HttpExceptionHandlingService(IJSRuntime jsRuntime, IMatDialogService matDialogService, ILogger<HttpExceptionHandlingService> logger)
{
    public async Task HandleHttpExceptionAsync(HttpRequestException exception, string friendlyErrorMessage)
    {
        #region null checks
        ArgumentNullException.ThrowIfNull(exception);

        if (string.IsNullOrEmpty(friendlyErrorMessage))
        {
            throw new ArgumentException($"'{nameof(friendlyErrorMessage)}' cannot be null or empty.", nameof(friendlyErrorMessage));
        }
        #endregion

        logger.HttpError(exception, friendlyErrorMessage);

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
