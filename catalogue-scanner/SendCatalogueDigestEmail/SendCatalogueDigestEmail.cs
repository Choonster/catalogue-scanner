using CatalogueScanner.Dto.Config;
using CatalogueScanner.Dto.FunctionResult;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using SendGrid.Helpers.Mail;
using System;

namespace CatalogueScanner
{
    public class SendCatalogueDigestEmail
    {
        private readonly EmailSettings settings;
        private readonly IStringLocalizer<SendCatalogueDigestEmail> S;

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

        public SendCatalogueDigestEmail(IOptionsSnapshot<CatalogueScannerSettings> settings, IStringLocalizer<SendCatalogueDigestEmail> stringLocalizer)
        {
            #region null checks
            if (settings is null)
            {
                throw new ArgumentNullException(nameof(settings));
            }
            #endregion

            this.settings = settings.Value.Email;
            S = stringLocalizer ?? throw new ArgumentNullException(nameof(stringLocalizer));
        }

        [FunctionName("SendCatalogueDigestEmail")]
        [return: SendGrid]
        public SendGridMessage Run([ActivityTrigger] Catalogue catalogue)
        {
            #region null checks
            if (catalogue is null)
            {
                throw new ArgumentNullException(nameof(catalogue));
            }
            #endregion

            var summary = S.Plural(catalogue.Items.Count, "Catalogue Scanner found 1 matching item at {1}", "Catalogue Scanner found {0} matching items at {1}", catalogue.Store);

            var message = new SendGridMessage()
            {
                From = new EmailAddress(FromEmail, FromName),
                Subject = summary,
            };

            message.AddTo(settings.ToEmail, settings.ToName);

            message.AddContent("text/plain", summary);

            return message;
        }
    }
}