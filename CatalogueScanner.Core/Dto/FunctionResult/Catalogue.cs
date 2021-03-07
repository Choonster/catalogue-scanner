using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;

namespace CatalogueScanner.Core.Dto.FunctionResult
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class Catalogue
    {
        public string Store { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
        public ICollection<CatalogueItem> Items { get; } = new List<CatalogueItem>();

        public Catalogue(string store, DateTimeOffset startDate, DateTimeOffset endDate, ICollection<CatalogueItem> items)
        {
            Store = store;
            StartDate = startDate;
            EndDate = endDate;
            Items = items;
        }
    }
}
