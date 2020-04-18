using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace CatalogueScanner.Dto.FunctionResult
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
