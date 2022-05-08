using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Globalization;

namespace CatalogueScanner.SaleFinder.Dto.FunctionResult
{
    /// <summary>
    /// Represents a SaleFinder catalogue to be scanned.
    /// </summary>
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class SaleFinderCatalogueDownloadInformation
    {
        /// <summary>
        /// The ID of the SaleFinder catalogue.
        /// </summary>
        public int SaleId { get; set; }

        /// <summary>
        /// The base URI of the SaleFinder catalogue page.
        /// </summary>
        public Uri BaseUri { get; set; }

        /// <summary>
        /// The store that the SaleFinder catalogue belongs to.
        /// </summary>
        public string Store { get; set; }

        /// <summary>
        /// The culture used to display prices in the digest email.
        /// </summary>
        public CultureInfo CurrencyCulture { get; set; }

        public SaleFinderCatalogueDownloadInformation(int saleId, Uri baseUri, string store, CultureInfo currencyCulture)
        {
            SaleId = saleId;
            BaseUri = baseUri;
            Store = store;
            CurrencyCulture = currencyCulture;
        }
    }
}
