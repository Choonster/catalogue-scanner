using CatalogueScanner.Core.Localisation;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PlaywrightSharp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CatalogueScanner.ColesOnline
{
    public class ColesPlaywrightTest
    {
        private const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4421.0 Safari/537.36";
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

            await page.AddInitScriptAsync(@"
                    // https://intoli.com/blog/not-possible-to-block-chrome-headless/test-headless-final.js
                    (function () {
debugger;
                        // Pass the Webdriver Test.
                        delete window.webdriver;                        

                        Object.defineProperty(navigator, 'webdriver', {
                            get: () => false,
                        });
                    
                        // Pass the Chrome Test.
                        // We can mock this in as much depth as we need for the test.
                        window.navigator.chrome = {
                            runtime: {},
                            // etc.
                        };
                    
                        // Pass the Permissions Test.
                        const originalQuery = window.navigator.permissions.query;
                        window.navigator.permissions.query = (parameters) => (
                            parameters.name === 'notifications' ?
                                Promise.resolve({ state: Notification.permission }) :
                                originalQuery(parameters)
                        );
                    
                        // Pass the Plugins Length Test.
                        // Overwrite the `plugins` property to use a custom getter.
                        Object.defineProperty(navigator, 'plugins', {
                            // This just needs to have `length > 0` for the current test,
                            // but we could mock the plugins too if necessary.
                            get: () => [1, 2, 3, 4, 5],
                        });
                    
                        // Pass the Languages Test.
                        // Overwrite the `plugins` property to use a custom getter.
                        Object.defineProperty(navigator, 'languages', {
                            get: () => ['en-US', 'en'],
                        });

                        //Object.defineProperty(window, 'webdriver', {
                        //    get: () => this.__webdriver,
                        //    set: (v) => {
                        //        debugger;
                        //        this.__webdriver = v;
                        //    },
                        //});

                        Object.seal(window);
                    })();
                ").ConfigureAwait(false);

            var webdriverBeforeNav = await DetectWebdriver(page).ConfigureAwait(false);

            var fingerprintRequestRegex = new Regex(@"/fingerprint$");

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

            const string colesUrl = "https://shop.coles.com.au/a/top-ryde/specials/browse";

            var initialPageResponse = await page.GoToAsync(colesUrl, LifecycleEvent.DOMContentLoaded, timeout: 0).ConfigureAwait(false);

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
                var firstCookies = await page.Context.GetCookiesAsync(colesUrl).ConfigureAwait(false);

                await page.WaitForTimeoutAsync((int)TimeSpan.FromSeconds(30).TotalMilliseconds).ConfigureAwait(false);

                var cookies = await page.Context.GetCookiesAsync(colesUrl).ConfigureAwait(false);

                var responseContent = await response.GetTextAsync().ConfigureAwait(false);

                var secondResponse = await page.GoToAsync(colesUrl).ConfigureAwait(false);

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
