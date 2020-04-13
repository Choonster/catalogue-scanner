using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace CatalogueScanner.Dto.StorageEntity
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class CatalogueItem
    {
        public string? Id { get; set; }

        public string? Sku { get; set; }

        public Uri? Uri { get; set; }

        public string? Name { get; set; }
    }
}
