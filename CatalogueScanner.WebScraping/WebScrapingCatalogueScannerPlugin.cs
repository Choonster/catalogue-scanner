﻿using CatalogueScanner.Configuration;
using CatalogueScanner.ConfigurationUI.Extensions;
using CatalogueScanner.Core.Host;
using CatalogueScanner.WebScraping.Options;
using CatalogueScanner.WebScraping.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using System;

namespace CatalogueScanner.WebScraping
{
    public class WebScrapingCatalogueScannerPlugin : ICatalogueScannerPlugin
    {
        public void Configure(ICatalogueScannerHostBuilder builder)
        {
            #region null checks
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            #endregion

            builder.Services.AddHttpClient();

            builder.Services.AddHttpClient(WebScrapingApiOptions.WebScrapingApi, (serviceProvider, httpClient) =>
            {
                var options = serviceProvider
                   .GetRequiredService<IOptions<WebScrapingApiOptions>>()
                   .Value;

                if (options.BaseAddress == null)
                {
                    throw new InvalidOperationException($"{WebScrapingApiOptions.WebScrapingApi}:{nameof(options.BaseAddress)} app setting must be configured");
                }

                httpClient.BaseAddress = options.BaseAddress;
            });

            builder.Services.AddHttpClient<ColesOnlineService>(WebScrapingApiOptions.WebScrapingApi, (httpClient) =>
            {
                var baseAddress = httpClient.BaseAddress ?? throw new InvalidOperationException($"{nameof(httpClient)}.{nameof(httpClient.BaseAddress)} is null");
                httpClient.BaseAddress = httpClient.BaseAddress.AppendPath("coles-online/");
            });

            builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.LocalConfiguration)
                .EnableTokenAcquisitionToCallDownstreamApi(options => builder.LocalConfiguration.GetSection(Constants.AzureAd).Bind(options))
                .AddInMemoryTokenCaches();

            AddConfiguration(builder);
        }

        private static void AddConfiguration(ICatalogueScannerHostBuilder builder)
        {
            var webScrapingLocal = builder.LocalConfiguration.GetSection("WebScraping");

            builder.Services
                .ConfigureOptions<WebScrapingApiOptions>(webScrapingLocal.GetSection(WebScrapingApiOptions.WebScrapingApi));
        }
    }
}
