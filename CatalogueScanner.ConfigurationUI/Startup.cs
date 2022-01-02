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
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using OrchardCore.Localization;
using OrchardCore.Localization.PortableObject;

namespace CatalogueScanner.ConfigurationUI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddMatBlazor();
            services.AddClipboard();

            services.AddMatToaster(config =>
            {
                config.Position = MatToastPosition.BottomCenter;
                config.PreventDuplicates = true;
                config.NewestOnTop = true;
                config.ShowCloseButton = false;
            });

            services.AddScoped<TokenProvider>();

            services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(Configuration.GetSection("AzureAd"))
                .EnableTokenAcquisitionToCallDownstreamApi()
                .AddInMemoryTokenCaches();

            services.Configure<CatalogueScannerApiOptions>(Configuration.GetSection(CatalogueScannerApiOptions.CatalogueScannerApi));

            services.AddCatalogueScannerApiHttpClient<CatalogueScanStateService>("CatalogueScanState");
            services.AddCatalogueScannerApiHttpClient<ManagementService>("Management");

            services.AddSingleton<ILocalizationFileLocationProvider, ContentRootPoFileLocationProvider>();

            services.AddScoped<TimeZoneService>();

            IFunctionsHostBuilder functionsHostBuilder = new DummyFunctionsHostBuilder(services);

            ICatalogueScannerHostBuilder catalogueScannerHostBuilder = new CatalogueScannerHostBuilder(functionsHostBuilder, Configuration, Configuration);

            catalogueScannerHostBuilder
                .AddPlugin<CoreCatalogueScannerPlugin>()
                .AddPlugin<OrchardCoreLocalisationCatalogueScannerPlugin>()
                .AddPlugin<SaleFinderCatalogueScannerPlugin>();


            string applicationInsightsConnectionString = Configuration["APPINSIGHTS_CONNECTIONSTRING"];
            if (!string.IsNullOrEmpty(applicationInsightsConnectionString))
            {
                services.AddApplicationInsightsTelemetry(applicationInsightsConnectionString);
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });

            logger.LogWarning("AppServicesAuthenticationInformation.IsAppServicesAadAuthenticationEnabled: {value}", AppServicesAuthenticationInformation.IsAppServicesAadAuthenticationEnabled);
        }



        private class DummyFunctionsHostBuilder : IFunctionsHostBuilder
        {
            public IServiceCollection Services { get; }

            public DummyFunctionsHostBuilder(IServiceCollection services)
            {
                Services = services;
            }
        }
    }
}
