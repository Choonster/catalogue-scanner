using CatalogueScanner.Core.Dto.FunctionResult;
using CatalogueScanner.Core.Functions.SendCatalogueDigestEmailTemplate;
using CatalogueScanner.Core.Localisation;
using CatalogueScanner.Core.Options;
using HtmlAgilityPack;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace CatalogueScanner.Core.Functions
{
    public class SendCatalogueDigestEmail
    {
        private readonly EmailOptions options;
        private readonly IPluralStringLocalizer<SendCatalogueDigestEmail> S;

        public SendCatalogueDigestEmail(IOptionsSnapshot<EmailOptions> optionsAccessor, IPluralStringLocalizer<SendCatalogueDigestEmail> pluralStringLocalizer)
        {
            #region null checks
            ArgumentNullException.ThrowIfNull(optionsAccessor);
            #endregion

            options = optionsAccessor.Value;
            S = pluralStringLocalizer ?? throw new ArgumentNullException(nameof(pluralStringLocalizer));
        }

        [Function(CoreFunctionNames.SendCatalogueDigestEmail)]
        [SendGridOutput]
        public SendGridMessage Run([ActivityTrigger] Catalogue filteredCatalogue)
        {
            #region null checks
            ArgumentNullException.ThrowIfNull(filteredCatalogue);
            #endregion

            var summary = S.Plural(filteredCatalogue.Items.Count, "Catalogue Scanner found 1 matching item at {1}", "Catalogue Scanner found {0} matching items at {1}", filteredCatalogue.Store);

            var message = new SendGridMessage
            {
                From = new EmailAddress(options.FromEmail, options.FromName),
                Subject = summary,
            };

            message.AddTo(options.ToEmail, options.ToName);

            var hasMultiBuyItems = filteredCatalogue.Items.Any(HasMultiBuyPrice);

            message.AddContent(MimeType.Html, GetHtmlContent(summary!, filteredCatalogue, hasMultiBuyItems));
            message.AddContent(MimeType.Text, GetPlainTextContent(summary!, filteredCatalogue, hasMultiBuyItems));

            return message;
        }

        private string GetHtmlContent(string summary, Catalogue filteredCatalogue, bool hasMultiBuyItems)
        {
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(EmailTemplates.HtmlTemplate);

            #region Title
            var titleNode = htmlDocument.DocumentNode
                        .Descendants("title")
                        .First();

            titleNode.AppendChild(htmlDocument.CreateTextNode(summary));
            #endregion

            #region Intro Text
            var introTextNodes = htmlDocument.DocumentNode
                        .Descendants()
                        .Where(node => node.HasClass("intro-text"));

            foreach (var node in introTextNodes)
            {
                node.AppendChild(htmlDocument.CreateTextNode(summary));
            }
            #endregion

            #region Items table
            var itemsTableContainerNode = htmlDocument.GetElementbyId("items-table-container");

            var table = itemsTableContainerNode.AppendChild(htmlDocument.CreateElement("table"));

            #region Table header
            var thead = table.AppendChild(htmlDocument.CreateElement("thead"));

            var headerRow = thead.AppendChild(htmlDocument.CreateElement("tr"));
            headerRow.AppendChild(ElementWithText("th", S["Product Name"]!));
            headerRow.AppendChild(ElementWithText("th", S["Price"]));

            if (hasMultiBuyItems)
            {
                headerRow.AppendChild(ElementWithText("th", S["Multi-Buy Price"]));
            }
            #endregion

            #region Table body
            var tbody = table.AppendChild(htmlDocument.CreateElement("tbody"));

            var itemRows = filteredCatalogue.Items
                .Select(catalogueItem =>
                {
                    var row = htmlDocument.CreateElement("tr");

                    var productNameCell = row.AppendChild(htmlDocument.CreateElement("td"));

                    if (catalogueItem.Uri != null)
                    {
                        var link = productNameCell.AppendChild(ElementWithText("a", ItemName(catalogueItem)));
                        link.SetAttributeValue("href", catalogueItem.Uri.AbsoluteUri);
                    }
                    else
                    {
                        productNameCell.AppendChild(htmlDocument.CreateTextNode(ItemName(catalogueItem)));
                    }

                    var priceCell = row.AppendChild(ElementWithText("td", Price(filteredCatalogue, catalogueItem.Price)));
                    priceCell.AddClass("price");

                    if (hasMultiBuyItems)
                    {
                        var multiBuyPriceCell = row.AppendChild(ElementWithText("td", MultiBuyPrice(filteredCatalogue, catalogueItem)));
                        multiBuyPriceCell.AddClass("multi-buy-price");
                    }

                    return row;
                });

            foreach (var row in itemRows)
            {
                tbody.AppendChild(row);
            }
            #endregion
            #endregion

            using var writer = new StringWriter();

            htmlDocument.Save(writer);

            return writer.ToString();

            HtmlNode ElementWithText(string elementName, string text)
            {
                var element = htmlDocument.CreateElement(elementName);
                element.AppendChild(htmlDocument.CreateTextNode(text));

                return element;
            }
        }

        private string GetPlainTextContent(string summary, Catalogue filteredCatalogue, bool hasMultiBuyItems)
        {
            var content = new StringBuilder(EmailTemplates.PlainTextTemplate);

            content.Replace("%SUMMARY%", summary);

            var list = string.Join(
                Environment.NewLine,
                filteredCatalogue.Items
                    .Select(catalogueItem =>
                    {
                        var row = $"- {ItemName(catalogueItem)}";

                        if (catalogueItem.Uri != null)
                        {
                            row += $" ({catalogueItem.Uri.AbsoluteUri})";
                        }

                        if (catalogueItem.Price != null)
                        {
                            row += $" - {Price(filteredCatalogue, catalogueItem.Price)}";
                        }

                        if (hasMultiBuyItems && HasMultiBuyPrice(catalogueItem))
                        {
                            row += $" - {MultiBuyPrice(filteredCatalogue, catalogueItem)}";
                        }

                        return row;
                    })
            );

            content.Replace("%ITEMSLIST%", list);

            return content.ToString();
        }

        private string ItemName(CatalogueItem catalogueItem) => catalogueItem.Name ?? S["Unknown Item"]!;

        private static string Price(Catalogue catalogue, decimal? price) =>
            price?.ToString("C", catalogue.CurrencyCulture) ?? string.Empty;

        private static bool HasMultiBuyPrice(CatalogueItem catalogueItem) =>
            catalogueItem.MultiBuyQuantity != null && catalogueItem.MultiBuyTotalPrice != null;

        private string MultiBuyPrice(Catalogue catalogue, CatalogueItem catalogueItem) =>
            HasMultiBuyPrice(catalogueItem)
                ? S[
                    "{0} for {1}",
                    catalogueItem.MultiBuyQuantity!.Value.ToString(CultureInfo.CurrentCulture),
                    Price(catalogue, catalogueItem.MultiBuyTotalPrice)
                ]
                : string.Empty;
    }
}