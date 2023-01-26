using CatalogueScanner.ColesOnline.Dto.ColesOnline;
using CatalogueScanner.ColesOnline.Dto.FunctionInput;
using CatalogueScanner.ColesOnline.Service;
using CatalogueScanner.Core.Dto.FunctionResult;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System.Globalization;

namespace CatalogueScanner.ColesOnline.Functions
{
    public class DownloadColesOnlineSpecialsPage
    {
        private readonly ColesOnlineService colesOnlineService;

        public DownloadColesOnlineSpecialsPage(ColesOnlineService colesOnlineService)
        {
            this.colesOnlineService = colesOnlineService;
        }

        [FunctionName(ColesOnlineFunctionNames.DownloadColesOnlineSpecialsPage)]
        public async Task<IEnumerable<CatalogueItem>> Run([ActivityTrigger] IDurableActivityContext context, CancellationToken cancellationToken)
        {
            #region null checks
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            #endregion

            var input = context.GetInput<DownloadColesOnlineSpecialsPageInput>();

            if (input is null)
            {
                throw new InvalidOperationException("Activity function input not present");
            }

            var productUrlTemplate = ColesOnlineService.ProductUrlTemplate;

            var response = await colesOnlineService.GetOnSpecialPageAsync(input.BuildId, input.Page, cancellationToken).ConfigureAwait(false);

            var items = response.PageProps
                !.SearchResults
                !.Results
                .OfType<Product>()
                .Select(product => new CatalogueItem
                {
                    Id = product.Id.ToString(CultureInfo.InvariantCulture),
                    Name = product.Name,
                    Uri = new Uri(productUrlTemplate.AbsoluteUri.Replace("[productId]", product.Id.ToString(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase)),
                    Price = product.Pricing?.Now,
                    MultiBuyQuantity = product.Pricing?.MultiBuyPromotion?.MinQuantity,
                    MultiBuyTotalPrice = product.Pricing?.MultiBuyPromotion?.Reward,
                })
               .ToList();

            return items;
        }
    }
}
