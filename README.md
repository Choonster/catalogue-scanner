# Catalogue Scanner

Catalogue Scanner is a .NET Core Azure Functions app that periodically checks online catalogues for specials matching configurable rules and sends a notification email when it finds matching items.

It currently only supports Coles and Woolworths catalogues, but the app has been built with extensibility in mind and support for more stores is planned.

There is also a configuration UI built with Blazor that can be used to edit the match rules and various other settings.

The app and its related resources can be hosted on Azure for around AU$5.00 per month.


## Deployment

### Catalogue Scanner

- In the Azure Portal, create a Function App with .NET 3.1 as the runtime stack.
	- The Consumption plan is recommended to keep costs low.
- Open the new Function App and click the "Get publish profile" button.
- Open the solution in Visual Studio, right click on **CatalogueScanner.DefaultHost** and select "Publish...".
- Click "New", select "Import Profile" and then select the downloaded publish settings file.
- Click "Publish" to publish to the Function App on Azure.
- Open the Function App and navigate to the "Configuration" pane.
- Add a new application setting with `CheckCatalogueFunctionCronExpression` as the name and a valid CRON expression as the value.
	- This setting controls how often the SaleFinder (Coles and Woolworths) catalogues are scanned.
	- `0 0 */6 * * *` scans every 6 hours.
- Add another application setting with `LocalisationCulture` as the name and a valid [.NET culture name](https://docs.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo?view=net-5.0#CultureNames) as the value.
	- This setting controls the language used in catalogue digest emails and log messages.
	- Examples includes `en-AU` for English (Australia), `en-US` for English (United States) and `ja-JP` for Japanese (Japan).
	- Not all languages have translations available; at the moment the only non-English language included is machine-translated French (`fr-FR`).

**CatalogueScanner.DefaultHost** can be customised to only include the plugins you want or replaced entirely with your own host application, the actual logic is provided by the referenced projects and plugins.


### Configuration

Catalogue Scanner stores its configuration in an Azure App Configuration instance.

- In the Azure Portal, create an App Configuration instance.
	- The Free plan can be used if you don't already have another Free instance in your subscription.
- Open the new App Configuration instance and navigate to the "Access keys" pane.
- Copy one of the connection strings.
- Open the Function App and navigate to the "Configuration" pane.
- Add a new application setting with `AzureAppConfigurationConnectionString` as the name and the connection string as the value.


### SendGrid

Catalogue Scanner uses SendGrid to send the catalogue digest emails. You can use any SendGrid account, but Azure's Free SendGrid plan includes more emails than SendGrid's normal Free plan (though it may not be available any more).

- In the Azure Portal, create a SendGrid account.
- Open the new SendGrid account and click "Manage" to open the SendGrid dashboard for the account.
- Open the Settings > API Keys page.
- Create an API Key with Restricted Access and only grant it the "Mail Send" permission.
- Copy the API Key and save it in a secure location so you can access it again if needed.
- Open the Function App and navigate to the "Configuration" pane.
- Add a new application setting with `AzureWebJobsSendGridApiKey` as the name and the API Key as the value.

You may also need to set up Sender Authentication in SendGrid before you can send emails.


### Configuration UI

The Catalogue Scanner Configuration UI is a .NET 5 Blazor Server app that can be hosted on any platform supported by Blazor, but hosting it in Azure App Service is the most convenient option.

- In the Azure Portal, create a Web App (App Service) with .NET 5 as the runtime stack.
	- The F1 (free) Dev/Test pricing tier is recommended to keep costs low.
- Open the new App Service and click the "Get publish profile" button.
- Open the solution in Visual Studio, right click on **CatalogueScanner.ConfigurationUI** and select "Publish...".
- Click "New", select "Import Profile" and then select the downloaded publish settings file.
- Add an [Azure SignalR service dependency](https://docs.microsoft.com/en-us/aspnet/core/signalr/publish-to-azure-web-app?view=aspnetcore-5.0).
- Click "Publish" to publish to the App Service on Azure.
- Open the App Service and navigate to the "Configuration" pane.
- Add a new application setting with `ConnectionStrings:AzureAppConfiguration` as the name and the Azure App Configuration connection string from earlier as the value.
- Add another application setting with `LocalisationCulture` as the name and a valid .NET culture name as the value.
	- This setting controls the language and date formats used in the UI.
	- See above for more details on valid values.
- Add another application setting with `CatalogueScannerApi:BaseAddress` as the name and the address of your Catalogue Scanner Function App followed by `/api/` as the value.
	- The value should look like `https://<your-catalogue-scanner-instance>.azurewebsites.net/api/` if you're not using a custom domain.


### Authentication

The Function App and App Service can use the built-in Azure AD authentication provided by Azure App Service. For the Configuration UI to access the Catalogue Scanner API, you should [manually register](https://docs.microsoft.com/en-au/azure/app-service/configure-authentication-provider-aad#advanced) them both with Azure AD and change the Configuration UI to request the `user_impersonation` permission in Catalogue Scanner.
