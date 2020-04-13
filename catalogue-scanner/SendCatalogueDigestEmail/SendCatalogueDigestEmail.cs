using CatalogueScanner.Dto.Config;
using CatalogueScanner.Dto.StorageEntity;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Options;
using SendGrid.Helpers.Mail;
using System;

namespace CatalogueScanner
{
    public class SendCatalogueDigestEmail
    {
        private readonly EmailSettings settings;

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
                From = new EmailAddress(settings.FromEmail, settings.FromName),
                Subject = $"Catalogue Scanner found {catalogue.Items.Count} matching item(s) at {catalogue.Store}",
            };

            message.AddTo(settings.ToEmail, settings.ToName);

            message.AddContent("text/plain", $"Catalogue Scanner found {catalogue.Items.Count} matching item(s) at {catalogue.Store}");

            return message;
        }
    }
}