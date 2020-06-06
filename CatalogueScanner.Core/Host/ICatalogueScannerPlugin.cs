namespace CatalogueScanner.Core.Host
{
    public interface ICatalogueScannerPlugin
    {
        void Configure(ICatalogueScannerHostBuilder builder);
    }
}
