using CatalogueScanner.Core.Dto.FunctionResult;
using CatalogueScanner.SaleFinder.Dto.FunctionInput;
using CatalogueScanner.SaleFinder.Service;
using HtmlAgilityPack;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CatalogueScanner.SaleFinder.Functions
{
    public partial class FillSaleFinderItemPrice(SaleFinderService saleFinderService, ILogger<FillSaleFinderItemPrice> logger)
    {
        [GeneratedRegex(@"(?:Any\s*)?(\d+)\s+for", RegexOptions.IgnoreCase, "en-AU")]
        private static partial Regex MultiBuyNForRegex();

        private static readonly ImmutableHashSet<string> SingleItemSaleOptionDescTexts = ["Any of these", "Now", "From"];

        private readonly SaleFinderService saleFinderService = saleFinderService;
        private readonly ILogger<FillSaleFinderItemPrice> logger = logger;

        [Function(SaleFinderFunctionNames.FillSaleFinderItemPrice)]
        public async Task<CatalogueItem> Run(
            [ActivityTrigger] FillSaleFinderItemPriceInput input,
            CancellationToken cancellationToken
        )
        {
            #region null checks
            ArgumentNullException.ThrowIfNull(input);
            #endregion

            var item = input.Item;

            if (item.Id is null)
            {
                return item;
            }

            var tooltipHtml = await saleFinderService.GetItemTooltipAsync(ParseLong(item.Id), cancellationToken).ConfigureAwait(false)
                ?? throw Error("tooltipHtml is null");

            var doc = new HtmlDocument();
            doc.LoadHtml(tooltipHtml);

            // Some items don't have prices, e.g. when they're out of stock
            var nowPriceSpan = DescendantByNameAndClass(doc.DocumentNode, "span", "sf-nowprice");

            if (nowPriceSpan is null)
            {
                return item;
            }

            var priceDisplaySpan = DescendantByNameAndClass(nowPriceSpan, "span", "sf-pricedisplay")
                ?? throw Error("Unabe to find span.sf-pricedisplay");

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

            // The .sf-saleoptiondesc span is only present for multi-buy prices (e.g. 2 for $10) or other promotional text (e.g. "Any of these")
            var saleOptionDescSpan = DescendantByNameAndClass(nowPriceSpan, "span", "sf-saleoptiondesc");

            if (saleOptionDescSpan is null)
            {
                return item with { Price = price };
            }

            var saleOptionDescText = saleOptionDescSpan.InnerText;

            if (SingleItemSaleOptionDescTexts.Contains(saleOptionDescText))
            {
                return item with { Price = price };
            }
            else if (MultiBuyNForRegex().Match(saleOptionDescText) is { Success: true } match)
            {
                return item with
                {
                    MultiBuyQuantity = ParseLong(match.Groups[1].Value),
                    MultiBuyTotalPrice = price,
                };
            }
            else
            {
                logger.LogError(Error($"Unknown format for span.sf-saleoptiondesc: \"{saleOptionDescText}\"").Message);
                return item;
            }

            InvalidOperationException Error(string message) => new($"Item ID {item!.Id}: {message}");
        }

        private static HtmlNode? DescendantByNameAndClass(HtmlNode node, string name, string className) =>
            node.Descendants(name).FirstOrDefault(node => node.HasClass(className));

        private static long ParseLong(string s) => long.Parse(s, CultureInfo.InvariantCulture);
        private static decimal ParseDecimal(string s) => decimal.Parse(s, CultureInfo.InvariantCulture);
    }
}
