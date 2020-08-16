using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CatalogueScanner.Configuration
{
    public static class ConfigurationExtensions
    {
        public static IServiceCollection ConfigureOptions<TOptions>(this IServiceCollection services, IConfigurationSection configuration) where TOptions : class, new()
        {
            services.Configure<TOptions>(configuration);

            services.AddScoped<ITypedConfiguration<TOptions>>(serviceProvider =>
                new TypedConfiguration<TOptions>(configuration)
            );

            services.AddScoped<IConfigurationService<TOptions>, ConfigurationService<TOptions>>();

            return services;
        }
    }
}
