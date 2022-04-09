using CatalogueScanner.Core.Dto.FunctionResult;
using CatalogueScanner.SaleFinder.Service;
using HtmlAgilityPack;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CatalogueScanner.SaleFinder.Functions
{
    public class FillSaleFinderItemPrice
    {
        private static readonly Regex MultiBuyNForRegex = new(@"(\d+) for");
        private static readonly Regex MultiBuyPriceRegex = new(@"\$(\d+.\d+)");

        private readonly SaleFinderService saleFinderService;

        public FillSaleFinderItemPrice(SaleFinderService saleFinderService)
        {
            this.saleFinderService = saleFinderService;
        }

        [FunctionName(SaleFinderFunctionNames.FillSaleFinderItemPrice)]
        public async Task<CatalogueItem> Run(
            [ActivityTrigger] CatalogueItem item,
            CancellationToken cancellationToken
        )
        {
            #region null checks
            if (item is null)
            {
                throw new ArgumentNullException(nameof(item));
            }
            #endregion

            if (item.Id is null)
            {
                return item;
            }

            var tooltipHtml = await saleFinderService.GetItemTooltipAsync(long.Parse(item.Id, CultureInfo.InvariantCulture), cancellationToken).ConfigureAwait(false);

            if (tooltipHtml is null)
            {
                throw new InvalidOperationException("tooltipHtml is null");
            }

            var doc = new HtmlDocument();
            doc.LoadHtml(tooltipHtml);

            var nowPriceSpan = DescendantByNameAndClass(doc.DocumentNode, "span", "sf-nowprice");

            if (nowPriceSpan is null)
            {
                throw new InvalidOperationException($"Unable to find span.sf-nowprice");
            }

            var dollarSpan = DescendantByNameAndClass(nowPriceSpan, "span", "sf-dollar");
            var centsSpan = DescendantByNameAndClass(nowPriceSpan, "span", "sf-cents");

            if (dollarSpan is not null && centsSpan is not null)
            {
                item.Price = decimal.Parse(dollarSpan.InnerText, CultureInfo.InvariantCulture) + (decimal.Parse(centsSpan.InnerText, CultureInfo.InvariantCulture) / 100);
            }

            var saleOptionDescSpan = DescendantByNameAndClass(nowPriceSpan, "span", "sf-saleoptiondesc");
            var priceDisplaySpan = DescendantByNameAndClass(nowPriceSpan, "span", "sf-pricedisplay");

            if (saleOptionDescSpan is not null && priceDisplaySpan is not null)
            {
                var saleOptionDescText = saleOptionDescSpan.InnerText;
                var multiBuyNForMatch = MultiBuyNForRegex.Match(saleOptionDescText);
                
                if (!multiBuyNForMatch.Success)
                {
                    throw new InvalidOperationException($"Unknown format for span.sf-saleoptiondesc: \"{saleOptionDescText}\"");
                }

                var priceDisplayText = priceDisplaySpan.InnerText;
                var multiBuyPriceMatch = MultiBuyPriceRegex.Match(priceDisplayText);

                if (!multiBuyPriceMatch.Success)
                {
                    throw new InvalidOperationException($"Unknown format for span.sf-pricedisplay: \"{priceDisplayText}\"");
                }

                item.MultiBuyQuantity = long.Parse(multiBuyNForMatch.Groups[1].Value, CultureInfo.InvariantCulture);
                item.MultiBuyTotalPrice = decimal.Parse(multiBuyPriceMatch.Groups[1].Value, CultureInfo.InvariantCulture);
            }

            return item;
        }

        private static HtmlNode? DescendantByNameAndClass(HtmlNode node, string name, string className) =>
            node.Descendants(name).FirstOrDefault(node => node.HasClass(className));
    }
}
