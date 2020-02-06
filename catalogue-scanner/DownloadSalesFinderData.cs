using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using CatalogueScanner.SalesFinder;

namespace CatalogueScanner
{
    public static class DownloadSalesFinderData
    {
        private static readonly HttpClient HttpClient = new HttpClient();

        private static readonly string SalesFinderUrl = "https://embed.salefinder.com.au/catalogue/svgData/{0}/?format=json";


        [FunctionName("DownloadSalesFinderData")]
        [return: Queue("catalogue-scanner-items", Connection = "StorageAccountConnectionString")]
        public static async Task<SalesFinderData> RunAsync([TimerTrigger("0 */1 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            var response = await HttpClient.GetAsync(string.Format(SalesFinderUrl, 31610));

            response.EnsureSuccessStatusCode();

            var resultString = await response.Content.ReadAsStringAsync();

            var result = SalesFinderData.FromJson(resultString.Trim('(', ')'));

            return result;
        }
    }
}
