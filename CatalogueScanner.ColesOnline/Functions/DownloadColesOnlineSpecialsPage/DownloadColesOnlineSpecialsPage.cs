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
                .Select(product =>
                    new CatalogueItem(
                        product.Id.ToString(CultureInfo.InvariantCulture),
                        product.Name,
                        null,
                        new Uri(productUrlTemplate.AbsoluteUri.Replace("[productId]", product.Id.ToString(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase)),
                        product.Pricing?.Now,
                        product.Pricing?.MultiBuyPromotion?.MinQuantity,
                        product.Pricing?.MultiBuyPromotion?.Reward
                    )
                )
               .ToList();

            return items;
        }
    }
}
