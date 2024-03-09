using CatalogueScanner.Configuration;
using CatalogueScanner.Core.Host;
using CatalogueScanner.WoolworthsOnline.Options;
using CatalogueScanner.WoolworthsOnline.Service;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;

namespace CatalogueScanner.WoolworthsOnline;

public class WoolworthsOnlineCatalogueScannerPlugin : ICatalogueScannerPlugin
{
    public void Configure(ICatalogueScannerHostBuilder builder)
    {
        #region null checks
        ArgumentNullException.ThrowIfNull(builder);
        #endregion

        builder.Services
            .AddHttpClient<WoolworthsOnlineService>(client =>
            {
                client.BaseAddress = new Uri("https://www.woolworths.com.au/apis/ui/");
                client.DefaultRequestHeaders.Accept.ParseAdd("application/json, text/plain, */*");
                client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-AU,en;q=0.9");
                client.DefaultRequestHeaders.CacheControl = CacheControlHeaderValue.Parse("no-cache");
                client.DefaultRequestHeaders.Pragma.ParseAdd("no-cache");
                client.DefaultRequestHeaders.Referrer = new Uri("https://www.woolworths.com.au/");
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
                client.DefaultRequestHeaders.Add("sec-ch-ua", "\"Not_A Brand\";v=\"8\", \"Chromium\";v=\"120\", \"Google Chrome\";v=\"120\"");
                client.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
                client.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\"Windows\"");
                client.DefaultRequestHeaders.Add("sec-fetch-dest", "empty");
                client.DefaultRequestHeaders.Add("sec-fetch-mode", "cors");
                client.DefaultRequestHeaders.Add("sec-fetch-site", "same-origin");
            });

        AddConfiguration(builder);
    }

    private static void AddConfiguration(ICatalogueScannerHostBuilder builder)
    {
        var woolworthsOnlineSection = builder.Configuration.GetSection("WoolworthsOnline");

        builder.Services
            .ConfigureOptions<WoolworthsOnlineOptions>(woolworthsOnlineSection.GetSection(WoolworthsOnlineOptions.WoolworthsOnline));
    }
}
