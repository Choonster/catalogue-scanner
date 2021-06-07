using CatalogueScanner.Core.Localisation;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using PlaywrightSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CatalogueScanner.ColesOnline
{
    public class ColesPlaywrightTest
    {
        private const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.72 Safari/537.36";
        private const string ColesUrl = "https://shop.coles.com.au/a/top-ryde/specials/browse";
        private readonly IOptionsSnapshot<FunctionsPathOptions> functionsPathOptions;



        public ColesPlaywrightTest(IOptionsSnapshot<FunctionsPathOptions> functionsPathOptions)
        {
            this.functionsPathOptions = functionsPathOptions;
        }

        [FunctionName("ColesPlaywrightTest")]
        public async Task RunAsync(
           [TimerTrigger("0 */2 * * * *")] TimerInfo timer,
           ILogger log
        )
        {
            var rootDirectory = functionsPathOptions.Value.RootDirectory;

            Environment.SetEnvironmentVariable(EnvironmentVariables.DriverPathEnvironmentVariable, rootDirectory);

            await Playwright.InstallAsync(browsersPath: rootDirectory).ConfigureAwait(false);

            using var playwright = await Playwright.CreateAsync().ConfigureAwait(false);
            await using var browser = await playwright.Chromium.LaunchAsync(headless: false, devtools: true).ConfigureAwait(false);
            var page = await browser.NewPageAsync(userAgent: UserAgent).ConfigureAwait(false);

            #region Add init script
            //            await page.AddInitScriptAsync(@"
            //                    // https://intoli.com/blog/not-possible-to-block-chrome-headless/test-headless-final.js
            //                    (function () {
            //debugger;
            //                        // Pass the Webdriver Test.
            //                        delete window.webdriver;                        

            //                        Object.defineProperty(navigator, 'webdriver', {
            //                            get: () => false,
            //                        });

            //                        // Pass the Chrome Test.
            //                        // We can mock this in as much depth as we need for the test.
            //                        window.navigator.chrome = {
            //                            runtime: {},
            //                            // etc.
            //                        };

            //                        // Pass the Permissions Test.
            //                        const originalQuery = window.navigator.permissions.query;
            //                        window.navigator.permissions.query = (parameters) => (
            //                            parameters.name === 'notifications' ?
            //                                Promise.resolve({ state: Notification.permission }) :
            //                                originalQuery(parameters)
            //                        );

            //                        // Pass the Plugins Length Test.
            //                        // Overwrite the `plugins` property to use a custom getter.
            //                        Object.defineProperty(navigator, 'plugins', {
            //                            // This just needs to have `length > 0` for the current test,
            //                            // but we could mock the plugins too if necessary.
            //                            get: () => [1, 2, 3, 4, 5],
            //                        });

            //                        // Pass the Languages Test.
            //                        // Overwrite the `plugins` property to use a custom getter.
            //                        Object.defineProperty(navigator, 'languages', {
            //                            get: () => ['en-US', 'en'],
            //                        });

            //                        //Object.defineProperty(window, 'webdriver', {
            //                        //    get: () => this.__webdriver,
            //                        //    set: (v) => {
            //                        //        debugger;
            //                        //        this.__webdriver = v;
            //                        //    },
            //                        //});


            //                        //navigator.serviceWorker.register('https://shop.coles.com.au/catalogue-scanner-service-worker.js');
            //                    })();
            //                ").ConfigureAwait(false); 
            #endregion

            //await page.RouteAsync("https://shop.coles.com.au/catalogue-scanner-service-worker.js", (route, request) =>
            //{
            //    route.FulfillAsync(
            //        status: HttpStatusCode.OK,
            //        contentType: "text/javascript",
            //        path: Path.Combine(rootDirectory, "service-worker.js")
            //    );
            //}).ConfigureAwait(false);

            var webdriverBeforeNav = await DetectWebdriver(page).ConfigureAwait(false);

            var fingerprintRequestRegex = new Regex(@"/fingerprint$");

            #region Replace true with false in fingerprint request
            //await page.RouteAsync(fingerprintRequestRegex, (route, request) =>
            //{
            //    var jsonStream = new MemoryStream();

            //    using (var writer = new Utf8JsonWriter(jsonStream, default)) {
            //        using var document = JsonDocument.Parse(request.PostData);

            //        writer.WriteStartObject();

            //        foreach (var property in document.RootElement.EnumerateObject())
            //        {
            //            if (property.Name == "d")
            //            {
            //                writer.WritePropertyName(property.Name);

            //                writer.WriteStartObject();

            //                var hasFoundFirstTrueValue = false;

            //                foreach (var innerProperty in property.Value.EnumerateObject())
            //                {
            //                    if (innerProperty.Value.ValueKind == JsonValueKind.True)
            //                    {
            //                        if (!hasFoundFirstTrueValue)
            //                        {
            //                            hasFoundFirstTrueValue = true;
            //                            innerProperty.WriteTo(writer);
            //                        }
            //                        else
            //                        {
            //                            writer.WriteBoolean(innerProperty.Name, false);
            //                        }
            //                    }
            //                    else
            //                    {
            //                        innerProperty.WriteTo(writer);
            //                    }
            //                }

            //                writer.WriteEndObject();
            //            }
            //            else
            //            {
            //                property.WriteTo(writer);
            //            }
            //        }

            //        writer.WriteEndObject();
            //    }

            //    jsonStream.Position = 0;

            //    using var reader = new StreamReader(jsonStream);
            //    var newJson = reader.ReadToEnd();

            //    route.ContinueAsync(postData: newJson);
            //}).ConfigureAwait(false);
            #endregion

            var hasMadeInitialRequest = 0;

            #region Replace initial page request
       /*     await page.RouteAsync(ColesUrl, (route, request) =>
            {
                if (Interlocked.CompareExchange(ref hasMadeInitialRequest, 1, 0) == 0)
                {
                    var cookieContainer = new CookieContainer();
                    const string cookieValue = "zpC7FSg0s11S5LZNpr2YWw==::xXERRdnjXdWzu/O1AnPCRfBVMi7eLMXI0qD3emH8UYZcFxBJpU1DnU7BgnjNKQ8Eu3wX6ss4wO4TE2weq2UXCV33BBYL5Sih++WRHIvPXLnRxwFNO0YTpyU7BB/3rMKyjm02zZVFBB5lfLVRu+IEIeJBBs2pjxUrgw0ToQXV0sN8OHyIJ/oS62k//PKaRpxFJa5T2fq/dTzrlhHqBcJ2kY3N40Z6u2/OVd2rPEe0kuQ9ARo3vlFFcADORbNPHWyTdEF0AgL76Szddfqz/wSsZ86zRHFMNbw8P8O1E6X6kOi/L9KlkUmZt++oVcF9BTXQmOhcDB6Xm3moJ+KbCrCW385aGhtK6yyd1mlqTl3n8uBahBWi4M0BgogCMHsB3oUtnVfFi7/eIbcrWwhzSuilqRgTvNulzkxMsUbFE2r/fn0GIdLN+KhRLAjiDVZjd3bjRW2ZDNBN9BEICj15qDhqJ38oHtZKVILq0VtbKwtxgo8puIcx117lsVtVm7GtpuUu0jKNjMDIOgB7EHnd202oNzSae1jjYNDXc331/2sYgZNadgojOutJj6EwxtliVyfIVr4tPaHujKVZNVTc09zz5PpkUpNKUNnk5y9q0AActi2Yf/xbScyaXFNjMN9uX06RUlADMTiJUXPib6nEiOJ4qrqFLUhdn3mxVy+PgInvCFYVGp79inmeYXrroVyq+fJXpn+5wA55+6cHgvUSG5aW2w==";

                    //cookieContainer.Add(new Cookie
                    //{
                    //    Name = "MK_iplz",
                    //    Value = cookieValue,
                    //    Domain = new Uri(ColesUrl).Host,
                    //    Path = "/",
                    //    Expires = DateTime.UtcNow.AddDays(1),
                    //    HttpOnly = true,
                    //});

                    var response = new HttpResponseMessage();
                    response.Headers.AddCookies(new[] {
                        new System.Net.Http.Headers.CookieHeaderValue("MK_iplz", cookieValue)
                        {
                            //Domain = new Uri(ColesUrl).Host,
                            Path = "/",
                            Expires = DateTime.UtcNow.AddDays(1),
                            HttpOnly = true,
                        }
                    });

                    string cookieHeader = response.Headers.GetValues(HeaderNames.SetCookie).FirstOrDefault();

                    //var cookieHeader = cookieContainer.GetCookieHeader(new Uri(request.Url));

                    route.FulfillAsync(
                        status: HttpStatusCode.OK,
                        path: Path.Combine(rootDirectory, "coles-initial-response.html"),
                        contentType: "text/html; charset=utf-8",
                        headers: new Dictionary<string, string>
                        {
                            ["Connection"] = "keep-alive",
                            ["Cache-Control"] = "no-cache, no-store, must-revalidate",
                            ["Expires"] = "0",
                            ["p3p"] = "CP=\"This site does not specify a policy in the P3P header\"",
                            ["Pragma"] = "no-cache",
                            ["Set-Cookie"] = cookieHeader,
                            //["X-Varnish"] = "545387701",
                            //["Age"] = "0",
                            //["Via"] = "1.1 varnish (Varnish/6.3)",
                            //["section-io-cache"] = "Miss",
                            //["section-io-id"] = "3aff13ebc327d820b9305f1d21e5b0bd",
                        }
                    );

                    return;
                }

                route.ContinueAsync();
            }).ConfigureAwait(false);*/
            #endregion

            #region Replace fingerprint request body
            await page.RouteAsync(fingerprintRequestRegex, (route, request) =>
            {
                var postData = request.GetPostDataJson();

                if (postData is null)
                {
                    throw new InvalidOperationException("Request's post data was null");
                }

                var originalFingerprintDoc = JsonDocument.Parse(ColesOnlineRequestResources.fingerprint_request_body);

                var originalFingerprintData = originalFingerprintDoc.RootElement.GetProperty("d").EnumerateObject().ToArray();

                var jsonStream = new MemoryStream();

                using (var writer = new Utf8JsonWriter(jsonStream, default))
                {
                    using var document = JsonDocument.Parse(request.PostData);

                    writer.WriteStartObject();

                    foreach (var property in document.RootElement.EnumerateObject())
                    {
                        if (property.Name == "d")
                        {
                            writer.WritePropertyName(property.Name);

                            writer.WriteStartObject();

                            var propertyIndex = 0;

                            foreach (var innerProperty in property.Value.EnumerateObject())
                            {
                                writer.WritePropertyName(innerProperty.Name);

                                var originalProperty = originalFingerprintData[propertyIndex];

                                originalProperty.Value.WriteTo(writer);

                                propertyIndex++;
                            }

                            writer.WriteEndObject();
                        }
                        else
                        {
                            property.WriteTo(writer);
                        }
                    }

                    writer.WriteEndObject();
                }

                jsonStream.Position = 0;

                using var reader = new StreamReader(jsonStream);
                var newJson = reader.ReadToEnd();

                route.ContinueAsync(postData: newJson);
            }).ConfigureAwait(false);
            #endregion

            var fingerprintRequestStarted = false;

            page.Request += (sender, e) =>
            {
                if (fingerprintRequestRegex.IsMatch(e.Request.Url))
                {
                    fingerprintRequestStarted = true;

                    log.LogInformation("Fingerprint request started");
                }

                log.LogInformation("Request to {url} started", e.Request.Url);
            };

            page.RequestFinished += (sender, e) =>
            {
                log.LogInformation("Request to {url} finished", e.Request.Url);
            };

            page.Response += (sender, e) =>
            {
                e.Response.GetTextAsync().ContinueWith((task) =>
                {
                    log.LogInformation("Response from {url} received with status {status} - {content}", e.Response.Url, e.Response.Status, task.Result);
                }, TaskScheduler.Current);
            };

            var initialPageResponse = await page.GoToAsync(ColesUrl, LifecycleEvent.DOMContentLoaded, timeout: 0).ConfigureAwait(false);

            var webdriverAfterNav = await DetectWebdriver(page).ConfigureAwait(false);

            log.LogInformation("Webdriver after nav: {webdriver} - Fingerprint request started: {fingerprintRequestStarted}", webdriverAfterNav, fingerprintRequestStarted);

            var pageLoadTask = page.WaitForLoadStateAsync(LifecycleEvent.Load);
            var fingerprintResponseTask = page.WaitForResponseAsync(fingerprintRequestRegex);

            await Task.WhenAll(pageLoadTask, fingerprintResponseTask).ConfigureAwait(false);

            var response = initialPageResponse;
            var fingerprintResponse = fingerprintResponseTask.Result;

            var fingerprintRequestJson = fingerprintResponse.Request.GetPostDataJson();

            if (response.Status == HttpStatusCode.TooManyRequests)
            {
                var firstCookies = await page.Context.GetCookiesAsync((string)ColesUrl).ConfigureAwait(false);

                await page.WaitForTimeoutAsync((int)TimeSpan.FromSeconds(30).TotalMilliseconds).ConfigureAwait(false);

                var cookies = await page.Context.GetCookiesAsync((string)ColesUrl).ConfigureAwait(false);

                var responseContent = await response.GetTextAsync().ConfigureAwait(false);

                var secondResponse = await page.GoToAsync(ColesUrl).ConfigureAwait(false);

                if (!secondResponse.Ok)
                {
                    throw new HttpRequestException($"Coles Online request failed with status {response.Status} ({response.StatusText})");
                }
            }

            await page.WaitForLoadStateAsync(LifecycleEvent.DOMContentLoaded).ConfigureAwait(false);
            await page.WaitForLoadStateAsync(LifecycleEvent.Networkidle).ConfigureAwait(false);

            await page.WaitForRequestAsync(new Regex(".+COLRSHomePage.+"), 0).ConfigureAwait(false);

            string content = await page.MainFrame.GetContentAsync().ConfigureAwait(false);

            dynamic productData = await page.EvaluateAsync<dynamic>(@"angular.element(document.querySelector(""[data-colrs-product-list]"")).data().$colrsProductListController.widget.data")
                                            .ConfigureAwait(false);

            static async Task<JsonElement?> DetectWebdriver(IPage page)
            {
                return await page.EvaluateAsync(@"
                    function () {
                        function detectWebdriver(_window) {
                            var navigatorHasWebdriver = _window.navigator.webdriver !== undefined && false !== _window.navigator.webdriver
                                , windowHasWebdriver = 'webdriver' in _window
                                , htmlHasWebDriver = 'true' === document.getElementsByTagName('html')[0].getAttribute('webdriver');
                    
                            return {
                                navigatorHasWebdriver,
                                windowHasWebdriver,
                                htmlHasWebDriver,
                            };
                        }
                    
                      return detectWebdriver(window); 
                    }
                ").ConfigureAwait(false);
            }
        }
    }
}
