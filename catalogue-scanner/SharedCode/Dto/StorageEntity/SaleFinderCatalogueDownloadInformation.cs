using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace CatalogueScanner.SharedCode.Dto.StorageEntity
{
    /// <summary>
    /// Represents a SaleFinder catalogue to be downloaded.
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
    }
}
