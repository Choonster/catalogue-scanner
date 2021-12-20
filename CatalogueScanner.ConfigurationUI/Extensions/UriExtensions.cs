using System;

namespace CatalogueScanner.ConfigurationUI.Extensions
{
    public static class UriExtensions
    {
        public static Uri AppendPath(this Uri uri, string path) => new(uri, new Uri(path, UriKind.Relative));
    }
}
