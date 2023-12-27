using Azure.Identity;
using CatalogueScanner.ColesOnline;
using CatalogueScanner.Configuration;
using CatalogueScanner.ConfigurationUI.Components;
using CatalogueScanner.ConfigurationUI.Extensions;
using CatalogueScanner.ConfigurationUI.Options;
using CatalogueScanner.ConfigurationUI.Service;
using CatalogueScanner.Core;
using CatalogueScanner.Core.Host;
using CatalogueScanner.Localisation.OrchardCore;
using CatalogueScanner.SaleFinder;
using CurrieTechnologies.Razor.Clipboard;
using MatBlazor;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using MudBlazor.Services;
using OrchardCore.Localization;
using OrchardCore.Localization.PortableObject;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;
var configuration = builder.Configuration;

#region Add App Configuration
var connectionString = configuration["ConnectionStrings:AzureAppConfiguration"]
  ?? throw new InvalidOperationException("ConnectionStrings:AzureAppConfiguration app setting not set");

configuration.AddCatalogueScannerAzureAppConfiguration(connectionString);

services.AddAzureAppConfiguration();
services.SetAzureAppConfigurationConnectionString(connectionString);

if (builder.Environment.IsProduction())
{
    var vaultUri = Environment.GetEnvironmentVariable("VaultUri")
      ?? throw new InvalidOperationException("VaultUri environment variable not set");

    configuration.AddAzureKeyVault(new Uri(vaultUri), new DefaultAzureCredential());
}
#endregion

#region Add Blazor Services
services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

services.AddRazorPages();
services.AddHttpContextAccessor();

services.AddMatBlazor();
services.AddMudServices();
services.AddClipboard();

services.AddMatToaster(config =>
{
    config.Position = MatToastPosition.BottomCenter;
    config.PreventDuplicates = true;
    config.NewestOnTop = true;
    config.ShowCloseButton = false;
});
#endregion

#region Add Catalogue Scanner API Services
services.AddScoped<TokenProvider>();

services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(configuration.GetSection("AzureAd"))
    .EnableTokenAcquisitionToCallDownstreamApi()
    .AddInMemoryTokenCaches();

services.Configure<CatalogueScannerApiOptions>(configuration.GetSection(CatalogueScannerApiOptions.CatalogueScannerApi));

services.AddCatalogueScannerApiHttpClient<CatalogueScanStateService>("CatalogueScanState");
services.AddCatalogueScannerApiHttpClient<ManagementService>("Management");
#endregion

#region Add Other Services
services.AddSingleton<ILocalizationFileLocationProvider, ContentRootPoFileLocationProvider>();

services.AddScoped<HttpExceptionHandlingService>();
services.AddScoped<TimeZoneService>();
#endregion

#region Add Catalogue Scanner Plugins
ICatalogueScannerHostBuilder catalogueScannerHostBuilder = new CatalogueScannerHostBuilder(configuration, services);

catalogueScannerHostBuilder
    .AddPlugin<CoreCatalogueScannerPlugin>()
    .AddPlugin<OrchardCoreLocalisationCatalogueScannerPlugin>()
    .AddPlugin<SaleFinderCatalogueScannerPlugin>()
    .AddPlugin<ColesOnlineCatalogueScannerPlugin>();
#endregion

#region Add Application Insights
var applicationInsightsConnectionString = configuration["APPINSIGHTS_CONNECTIONSTRING"];
if (!string.IsNullOrEmpty(applicationInsightsConnectionString))
{
    services.AddApplicationInsightsTelemetry(options =>
    {
        options.ConnectionString = applicationInsightsConnectionString;
    });
}
#endregion

var app = builder.Build();

#region Configure HTTP Request Pipeline
app.Logger.IsAppServicesAadAuthenticationEnabled(AppServicesAuthenticationInformation.IsAppServicesAadAuthenticationEnabled);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapRazorPages();

app.Run();
#endregion