using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CatalogueScanner.Configuration
{
    public static class ConfigurationExtensions
    {
        public static IServiceCollection ConfigureOptions<TOptions>(this IServiceCollection services, IConfigurationSection configuration, bool validateDataAnnotations = true) where TOptions : class, new()
        {
            var optionsBuilder = services.AddOptions<TOptions>()
                .Bind(configuration);

            if (validateDataAnnotations)
            {
                optionsBuilder.ValidateDataAnnotations();
            }

            services.AddSingleton<IOptionsChangeTokenSource<TOptions>>(new ConfigurationChangeTokenSource<TOptions>(Microsoft.Extensions.Options.Options.DefaultName, configuration));

            services.AddScoped<ITypedConfiguration<TOptions>>(serviceProvider =>
                new TypedConfiguration<TOptions>(configuration)
            );

            services.AddScoped<IConfigurationSaver<TOptions>, ConfigurationSaver<TOptions>>();

            return services;
        }
    }
}
