using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;

namespace CatalogueScanner.Dto.FunctionResult
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class Catalogue
    {
        public string Store { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
        public List<CatalogueItem> Items { get; } = new List<CatalogueItem>();

        public Catalogue(string store, DateTimeOffset startDate, DateTimeOffset endDate, List<CatalogueItem> items)
        {
            Store = store;
            StartDate = startDate;
            EndDate = endDate;
            Items = items;
        }
    }
}
