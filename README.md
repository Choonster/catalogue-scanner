# Catalogue Scanner

Catalogue Scanner is a .NET 6 Azure Functions app that periodically checks online catalogues for specials matching configurable rules and sends a notification email when it finds matching items.

It currently only supports Coles and Woolworths catalogues, but the app has been built with extensibility in mind and support for more stores is planned.

There is also a configuration UI built with Blazor that can be used to edit the match rules and various other settings.

The app and its related resources can be hosted on Azure for around AU$5.00 per month.

## Deployment

### SendGrid

Catalogue Scanner uses SendGrid to send the catalogue digest emails. You can use any SendGrid account, but it's easiest to create through Azure.

- In the Azure Portal, create a SendGrid account.
- Open the new SendGrid account and click "Manage" to open the SendGrid dashboard for the account.
- Open the Settings > API Keys page.
- Create an API Key with Restricted Access and only grant it the "Mail Send" permission.
- Copy the API Key so you can use it in the following deployment steps and save it in a secure location so you can access it again if needed.

You may also need to set up Sender Authentication in SendGrid before you can send emails.

### Application Resources

Catalogue Scanner and the Configuration UI can be deployed to Azure with their related resources using the **arm/main.bicep** ARM template, the parameters are explained in the template file. See [this page](https://docs.microsoft.com/en-au/azure/azure-resource-manager/bicep/deploy-to-resource-group) for more details about how to use the template.

**arm/deploy.ps1** can also be used as a convenient way to deploy the template directly from the PowerShell command-line. **arm/parameters/deployment-test.parameters.json** is an example of a parameter file that can be used with the template.

The template uses the lowest possible SKUs for Catalogue Scanner and Configuration UI (**Consumption** and **Free**, respectively); but uses **Standard** for the Azure App Configuration instance, this can be changed to **Free** if you don't already have a Free instance in the subscription.

The Configuration UI is a Blazor Server app that can be hosted on any platform supported by Blazor, but hosting it in Azure App Service is the most convenient option and is what the template does.

### Application Code

The application code can be deployed to Azure using GitHub Actions, see the workflows in the **.github** folder for examples of this. Deployment from other CI/CD services should also be possible, but Catalogue Scanner must not be deployed from Windows as Playwright (a dependency used for web scraping) relies on Linux file permissions to be set.

**CatalogueScanner.DefaultHost** can be customised to only include the plugins you want or replaced entirely with your own host application, the actual logic is provided by the referenced projects and plugins.

### Authentication

The Function App and App Service can use the built-in Azure AD authentication provided by Azure App Service. For the Configuration UI to access the Catalogue Scanner API, you should [manually register](https://docs.microsoft.com/en-au/azure/app-service/configure-authentication-provider-aad#advanced) them both with Azure AD and change the Configuration UI to request the `user_impersonation` permission in Catalogue Scanner.
