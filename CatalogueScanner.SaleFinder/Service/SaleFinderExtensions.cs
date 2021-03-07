using CatalogueScanner.SaleFinder.Serialisation;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;

namespace CatalogueScanner.SaleFinder.Service
{
    public static class SaleFinderExtensions
    {
        private static readonly MediaTypeFormatterCollection saleFinderMediaTypeFormatters = new MediaTypeFormatterCollection(
            new MediaTypeFormatter[] { new SaleFinderJsonMediaTypeFormatter() }
        );

        public static async Task<T> ReadSaleFinderResponseAsAync<T>(this HttpContent content)
        {
            #region null checks
            if (content is null)
            {
                throw new ArgumentNullException(nameof(content));
            }
            #endregion

            try
            {
                return await content.ReadAsAsync<T>(saleFinderMediaTypeFormatters).ConfigureAwait(false);
            }
            catch (JsonReaderException ex)
            {
                string? stringContent = null;
                try
                {
                    stringContent = await content.ReadAsStringAsync().ConfigureAwait(false);
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    // Ignored
                }

                if (!string.IsNullOrEmpty(stringContent))
                {
                    throw new JsonSerializationException("Failed to parse JSON. Original content: " + stringContent, ex);
                }

                throw;
            }
        }
    }
}
