using CatalogueScanner.Core.Dto.FunctionResult;
using CatalogueScanner.SaleFinder.Dto.FunctionInput;
using CatalogueScanner.SaleFinder.Service;
using HtmlAgilityPack;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
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
        private static readonly Regex MultiBuyNForRegex = new(@"(?:Any\s*)?(\d+)\s+for");

        private readonly SaleFinderService saleFinderService;

        public FillSaleFinderItemPrice(SaleFinderService saleFinderService)
        {
            this.saleFinderService = saleFinderService;
        }

        [FunctionName(SaleFinderFunctionNames.FillSaleFinderItemPrice)]
        public async Task<CatalogueItem> Run(
            [ActivityTrigger] FillSaleFinderItemPriceInput input,
            ILogger log,
            CancellationToken cancellationToken
        )
        {
            #region null checks
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input));
            }
            #endregion

            var item = input.Item;

            if (item.Id is null)
            {
                return item;
            }

            var tooltipHtml = await saleFinderService.GetItemTooltipAsync(ParseLong(item.Id), cancellationToken).ConfigureAwait(false);

            if (tooltipHtml is null)
            {
                throw Error("tooltipHtml is null");
            }

            var doc = new HtmlDocument();
            doc.LoadHtml(tooltipHtml);

            // Some items don't have prices, e.g. when they're out of stock
            var nowPriceSpan = DescendantByNameAndClass(doc.DocumentNode, "span", "sf-nowprice");

            if (nowPriceSpan is null)
            {
                return item;
            }

            var priceDisplaySpan = DescendantByNameAndClass(nowPriceSpan, "span", "sf-pricedisplay");

            if (priceDisplaySpan is null)
            {
                throw Error("Unabe to find span.sf-pricedisplay");
            }

            // Woolworths uses individual dollar and cents spans inside .sf-pricedisplay, other stores have the price with dollar sign as the inner text of .sf-pricedisplay
            decimal price;

            var dollarSpan = DescendantByNameAndClass(priceDisplaySpan, "span", "sf-dollar");
            var centsSpan = DescendantByNameAndClass(priceDisplaySpan, "span", "sf-cents");

            if (dollarSpan is not null && centsSpan is not null)
            {
                price = ParseDecimal(dollarSpan.InnerText) + (ParseDecimal(centsSpan.InnerText) / 100);
            }
            else
            {
                price = decimal.Parse(priceDisplaySpan.InnerText, NumberStyles.Currency, input.CurrencyCulture);
            }

            // The .sf-saleoptiondesc span is only present for multi-buy prices (e.g. 2 for $10)
            var saleOptionDescSpan = DescendantByNameAndClass(nowPriceSpan, "span", "sf-saleoptiondesc");

            if (saleOptionDescSpan is not null)
            {
                var saleOptionDescText = saleOptionDescSpan.InnerText;
                var multiBuyNForMatch = MultiBuyNForRegex.Match(saleOptionDescText);

                if (multiBuyNForMatch.Success)
                {
                    item.MultiBuyQuantity = ParseLong(multiBuyNForMatch.Groups[1].Value);
                    item.MultiBuyTotalPrice = price;
                }
                else
                {
                    log.LogError(Error($"Unknown format for span.sf-saleoptiondesc: \"{saleOptionDescText}\"").Message);
                }
            }
            else
            {
                item.Price = price;
            }

            return item;

            InvalidOperationException Error(string message) => new($"Item ID {item!.Id}: {message}");
        }

        private static HtmlNode? DescendantByNameAndClass(HtmlNode node, string name, string className) =>
            node.Descendants(name).FirstOrDefault(node => node.HasClass(className));

        private static long ParseLong(string s) => long.Parse(s, CultureInfo.InvariantCulture);
        private static decimal ParseDecimal(string s) => decimal.Parse(s, CultureInfo.InvariantCulture);
    }
}
