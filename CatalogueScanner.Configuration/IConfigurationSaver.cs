namespace CatalogueScanner.Configuration;

public interface IConfigurationSaver<TOptions>
{
    Task SaveAsync(TOptions options, CancellationToken cancellationToken = default);
}