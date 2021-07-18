using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;

namespace CatalogueScanner.WebScraping.Functions
{
    public class CheckColesOnlineSpecials
    {
        private readonly HttpClient httpClient;

        public CheckColesOnlineSpecials(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        [FunctionName("ScanColesOnline")]
        public static void Run([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            // POST to API?
        }
    }
}
