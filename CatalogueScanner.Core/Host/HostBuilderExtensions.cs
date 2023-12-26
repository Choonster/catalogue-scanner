using System;

namespace CatalogueScanner.Core.Host;

public static class HostBuilderExtensions
{
    public static ICatalogueScannerHostBuilder AddPlugin<T>(this ICatalogueScannerHostBuilder builder) where T : ICatalogueScannerPlugin, new()
    {
        #region null checks
        ArgumentNullException.ThrowIfNull(builder);
        #endregion

        new T().Configure(builder);

        return builder;
    }
}
