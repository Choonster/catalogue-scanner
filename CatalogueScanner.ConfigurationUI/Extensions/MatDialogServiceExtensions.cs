using System;
using System.Threading.Tasks;

namespace MatBlazor
{
    public static class MatDialogServiceExtensions
    {
        public static async Task<object> OpenFullPageAsync(this IMatDialogService matDialogService, Type componentType, MatDialogOptions options)
        {
            options ??= new MatDialogOptions();

            options.SurfaceClass = (options.SurfaceClass ?? string.Empty) + " full-page-dialog";
            options.SurfaceClass = options.SurfaceClass.Trim();

            return await matDialogService.OpenAsync(componentType, options);
        }
    }
}
