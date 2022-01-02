using CatalogueScanner.ConfigurationUI.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace CatalogueScanner.ConfigurationUI.Extensions
{
    public static class CatalogueScannerApiServiceCollectionExtensions
    {
        public static void AddCatalogueScannerApiHttpClient<TClient>(this IServiceCollection services, string subPath) where TClient : class =>
            services.AddHttpClient<TClient>((serviceProvider, httpClient) =>
            {
                var options = serviceProvider
                    .GetRequiredService<IOptions<CatalogueScannerApiOptions>>()
                    .Value;

                if (options.BaseAddress == null)
                {
                    throw new InvalidOperationException($"{CatalogueScannerApiOptions.CatalogueScannerApi}:{nameof(options.BaseAddress)} app setting must be configured");
                }

                httpClient.BaseAddress = options.BaseAddress.AppendPath(subPath + "/");
            });
    }
}
