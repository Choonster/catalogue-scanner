using CatalogueScanner.Dto.Config;
using CatalogueScanner.Dto.FunctionResult;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Options;
using SendGrid.Helpers.Mail;
using System;

namespace CatalogueScanner
{
    public class SendCatalogueDigestEmail
    {
        private readonly EmailSettings settings;

        private static string FromEmail
        {
            get
            {
                var hostName = Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME");

                if (hostName is null)
                {
                    throw new InvalidOperationException("WEBSITE_HOSTNAME environment variable not set");
                }

                var hostUri = new UriBuilder(Uri.UriSchemeHttps + Uri.SchemeDelimiter + hostName).Uri;

                return $"catalogue-scanner@{hostUri.Host}";
            }
        }

        private static string FromName => "Catalogue Scanner";

        public SendCatalogueDigestEmail(IOptionsSnapshot<CatalogueScannerSettings> settings)
        {
            #region null checks
            if (settings is null)
            {
                throw new ArgumentNullException(nameof(settings));
            }
            #endregion

            this.settings = settings.Value.Email;
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

            var message = new SendGridMessage()
            {
                From = new EmailAddress(FromEmail, FromName),
                Subject = $"Catalogue Scanner found {catalogue.Items.Count} matching item(s) at {catalogue.Store}",
            };

            message.AddTo(settings.ToEmail, settings.ToName);

            message.AddContent("text/plain", $"Catalogue Scanner found {catalogue.Items.Count} matching item(s) at {catalogue.Store}");

            return message;
        }
    }
}