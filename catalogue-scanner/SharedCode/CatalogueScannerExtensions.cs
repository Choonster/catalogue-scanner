﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;

namespace CatalogueScanner
{
    public static class CatalogueScannerExtensions
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
                string stringContent = null;
                try
                {
                    stringContent = await content.ReadAsStringAsync().ConfigureAwait(false);
                }
                catch
                {
                    // Ignored
                }

                if (!string.IsNullOrEmpty(stringContent))
                {
                    throw new Exception("Failed to parse JSON. Original content: " + stringContent, ex);
                }

                throw;
            }
        }
    }
}