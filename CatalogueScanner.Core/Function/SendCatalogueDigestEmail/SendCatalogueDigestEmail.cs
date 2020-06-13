using CatalogueScanner.Core.Dto.FunctionResult;
using CatalogueScanner.Core.Function.SendCatalogueDigestEmailTemplate;
using CatalogueScanner.Core.Localisation;
using CatalogueScanner.Core.Options;
using HtmlAgilityPack;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace CatalogueScanner.Core.Function
{
    public class SendCatalogueDigestEmail
    {
        private readonly EmailOptions options;
        private readonly IPluralStringLocalizer<SendCatalogueDigestEmail> S;

        private string FromEmail
        {
            get
            {
                var hostName = Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME");

                if (hostName is null)
                {
                    throw new InvalidOperationException(S["WEBSITE_HOSTNAME environment variable not set"]);
                }

                var hostUri = new UriBuilder(Uri.UriSchemeHttps + Uri.SchemeDelimiter + hostName).Uri;

                return $"catalogue-scanner@{hostUri.Host}";
            }
        }

        private static string FromName => "Catalogue Scanner";

        public SendCatalogueDigestEmail(IOptions<EmailOptions> optionsAccessor, IPluralStringLocalizer<SendCatalogueDigestEmail> pluralStringLocalizer)
        {
            #region null checks
            if (optionsAccessor is null)
            {
                throw new ArgumentNullException(nameof(optionsAccessor));
            }
            #endregion

            options = optionsAccessor.Value;
            S = pluralStringLocalizer ?? throw new ArgumentNullException(nameof(pluralStringLocalizer));
        }

        [FunctionName("SendCatalogueDigestEmail")]
        [return: SendGrid]
        public SendGridMessage Run([ActivityTrigger] Catalogue filteredCatalogue)
        {
            #region null checks
            if (filteredCatalogue is null)
            {
                throw new ArgumentNullException(nameof(filteredCatalogue));
            }
            #endregion

            var summary = S.Plural(filteredCatalogue.Items.Count, "Catalogue Scanner found 1 matching item at {1}", "Catalogue Scanner found {0} matching items at {1}", filteredCatalogue.Store);

            var message = new SendGridMessage()
            {
                From = new EmailAddress(FromEmail, FromName),
                Subject = summary,
            };

            message.AddTo(options.ToEmail, options.ToName);

            message.AddContent(MimeType.Html, GetHtmlContent(summary, filteredCatalogue));
            message.AddContent(MimeType.Text, GetPlainTextContent(summary, filteredCatalogue));

            return message;
        }

        private string GetHtmlContent(string summary, Catalogue filteredCatalogue)
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
            headerRow.AppendChild(ElementWithText("th", S["Product Name"]));
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

        private string GetPlainTextContent(string summary, Catalogue filteredCatalogue)
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

                        return row;
                    })
            );

            content.Replace("%ITEMSLIST%", list);

            return content.ToString();
        }

        private string ItemName(CatalogueItem catalogueItem) => catalogueItem.Name ?? S["Unknown Item"];
    }
}