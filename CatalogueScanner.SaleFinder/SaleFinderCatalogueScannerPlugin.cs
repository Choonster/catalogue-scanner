using CatalogueScanner.Configuration;
using CatalogueScanner.Core.Host;
using CatalogueScanner.SaleFinder.Options;
using CatalogueScanner.SaleFinder.Service;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace CatalogueScanner.SaleFinder
{
    public class SaleFinderCatalogueScannerPlugin : ICatalogueScannerPlugin
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

            builder.Services
                .AddHttpClient<SaleFinderService>(client =>
                {
                    client.BaseAddress = new Uri("https://embed.salefinder.com.au/");
                    client.DefaultRequestHeaders.Add("sec-fetch-site", "cross-site");
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.110 Safari/537.36");
                })
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                });

            AddConfiguration(builder);
        }

        private static void AddConfiguration(ICatalogueScannerHostBuilder builder)
        {
            var saleFinderSection = builder.Configuration.GetSection("SaleFinder");

            builder.Services
                .ConfigureOptions<ColesOptions>(saleFinderSection.GetSection(ColesOptions.Coles))
                .ConfigureOptions<WoolworthsOptions>(saleFinderSection.GetSection(WoolworthsOptions.Woolworths));
        }
    }
}
