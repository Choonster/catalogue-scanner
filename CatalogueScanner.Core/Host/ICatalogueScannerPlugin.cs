namespace CatalogueScanner.Core.Host
{
    public interface ICatalogueScannerPlugin
    {
        void Register(ICatalogueScannerHostBuilder hostBuilder);
    }
}
