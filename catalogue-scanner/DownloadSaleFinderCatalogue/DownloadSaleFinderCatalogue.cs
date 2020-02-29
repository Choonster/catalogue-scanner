using CatalogueScanner.Dto.SaleFinder;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CatalogueScanner.DownloadSaleFinderCatalogue
{
    public class DownloadSaleFinderCatalogue
    {
        private const string SaleFinderUrl = "https://embed.salefinder.com.au/catalogue/svgData/{0}/?format=json";

        private readonly HttpClient HttpClient;

        public DownloadSaleFinderCatalogue(IHttpClientFactory httpClientFactory)
        {
            HttpClient = httpClientFactory.CreateClient();
        }

        [FunctionName("DownloadSalesFinderData")]
        public async Task RunAsync(
            [TimerTrigger("0 */1 * * * *")]
            TimerInfo myTimer,
            ILogger log,
            [Queue(Constants.QueueNames.DownloadedItems, Connection = "StorageAccountConnectionString")] ICollector<Item> collector
        )
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            var response = await HttpClient.GetAsync(new Uri(string.Format(CultureInfo.InvariantCulture, SaleFinderUrl, 31610))).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadSaleFinderResponseAsAync<Catalogue>().ConfigureAwait(false);

            log.LogInformation($"Successfully downloaded and parsed catalogue with {result.Pages.Count} pages");

            Parallel.ForEach(
                result.Pages.SelectMany(page => page.Items),
                item => collector.Add(item)
            );
        }
    }
}
