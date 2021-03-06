﻿using System;
using System.Threading.Tasks;

namespace MatBlazor
{
    public static class MatDialogServiceExtensions
    {
        public static async Task<object> OpenFullPageAsync(this IMatDialogService matDialogService, Type componentType, MatDialogOptions options)
        {
            #region null checks
            if (matDialogService is null)
            {
                throw new ArgumentNullException(nameof(matDialogService));
            }

            if (componentType is null)
            {
                throw new ArgumentNullException(nameof(componentType));
            }
            #endregion

            options ??= new MatDialogOptions();

            options.SurfaceClass = (options.SurfaceClass ?? string.Empty) + " full-page-dialog";
            options.SurfaceClass = options.SurfaceClass.Trim();

            return await matDialogService.OpenAsync(componentType, options).ConfigureAwait(true);
        }
    }
}
