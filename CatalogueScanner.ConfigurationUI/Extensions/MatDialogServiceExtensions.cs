using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace MatBlazor;

public static class MatDialogServiceExtensions
{
    public static async Task<object> OpenFullPageAsync<TComponent>(this IDialogService dialogService, string title, DialogOptions? options) where TComponent : IComponent
    {
        #region null checks
        ArgumentNullException.ThrowIfNull(dialogService);
        #endregion

        options ??= new DialogOptions();

        options.ClassBackground = (options.ClassBackground ?? string.Empty) + " full-page-dialog";
        options.ClassBackground = options.ClassBackground.Trim();

        return await dialogService.ShowAsync<TComponent>(title, options).ConfigureAwait(true);
    }
}
