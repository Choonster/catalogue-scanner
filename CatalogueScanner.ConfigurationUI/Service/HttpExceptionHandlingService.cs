using Microsoft.Identity.Web;
using Microsoft.JSInterop;
using MudBlazor;
using System.Net;

namespace CatalogueScanner.ConfigurationUI.Service;

public class HttpExceptionHandlingService(IJSRuntime jsRuntime, IDialogService dialogService, ILogger<HttpExceptionHandlingService> logger, TokenProvider tokenProvider)
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

        if (exception.StatusCode == HttpStatusCode.Unauthorized && AppServicesAuthenticationInformation.IsAppServicesAadAuthenticationEnabled)
        {
            fullMessage += "\n\nThis may have happened because your session has expired. Do you want to clear your session and refresh the page?";

            fullMessage += $"\n\nToken: {tokenProvider.AccessToken}";

            var shouldRefresh = await dialogService.ShowMessageBox(friendlyErrorMessage, fullMessage, yesText: "Yes", noText: "No").ConfigureAwait(false);

            if (shouldRefresh == true)
            {
                await jsRuntime.InvokeVoidAsync("blazorClearAppServicesAuthenticationSession").ConfigureAwait(false);
            }
        }
        else
        {
            await dialogService.ShowMessageBox(friendlyErrorMessage, fullMessage).ConfigureAwait(false);
        }
    }
}
