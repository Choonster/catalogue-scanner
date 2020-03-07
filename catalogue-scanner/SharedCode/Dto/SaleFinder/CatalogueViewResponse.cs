using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CatalogueScanner.Dto.SaleFinder
{
    /// <summary>
    /// Returned from the SaleFinder Catalogue View request:
    /// https://embed.salefinder.com.au/catalogues/view/{storeId}/?locationId={locationId}
    /// </summary>
    public partial class CatalogueViewResponse
    {
        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("breadcrumb")]
        public string Breadcrumb { get; set; }

        [JsonProperty("region")]
        public string Region { get; set; }

        [JsonProperty("redirect")]
        public string Redirect { get; set; }
    }
}
