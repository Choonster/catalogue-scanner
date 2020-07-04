﻿// Originally generated by quicktype (https://quicktype.io/), then manually cleaned up

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace CatalogueScanner.SaleFinder.Dto.SaleFinder
{
    /// <summary>
    /// Returned from the SaleFinder Catalogue SVG Data request:
    /// https://embed.salefinder.com.au/catalogue/svgData/{saleId}/?format=json
    /// </summary>
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public partial class SaleFinderCatalogue
    {
        public string? Content { get; set; }
        public string? Breadcrumb { get; set; }
        public string? AreaName { get; set; }
        public string? SaleDescription { get; set; }
        public string? SaleName { get; set; }
        public string? YoutubeId { get; set; }
        public DateTimeOffset PublishDate { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }

        [JsonProperty("catalogue", ItemConverterType = typeof(PageConverter))]
        public List<Page> Pages { get; } = new List<Page>();
    }

    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    public partial class Page
    {
        [JsonIgnore]
        public List<Item> Items { get; } = new List<Item>();

        [JsonProperty("imagefile")]
        public string? ImageFile { get; set; }

        public long? ImageWidth { get; set; }
        public double? ImageHeight { get; set; }
    }

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public partial class Item
    {
        public string? VideoId { get; set; }
        public Shape Shape { get; set; }
        public List<long> Coords { get; } = new List<long>();

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public long? ItemId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string? Href { get; set; }

        [JsonProperty("SKU", NullValueHandling = NullValueHandling.Ignore)]
        public string? Sku { get; set; }

        public long? SystemId { get; set; }
        public object? ExtraId { get; set; }
        public object? Extra2Id { get; set; }

        [JsonProperty("extraURL", NullValueHandling = NullValueHandling.Ignore)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1056:Uri properties should not be strings", Justification = "SaleFinder may not output valid URIs for this property")]
        public string? ExtraUrl { get; set; }

        [JsonProperty("extraURLText", NullValueHandling = NullValueHandling.Ignore)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1056:Uri properties should not be strings", Justification = "This property probably isn't a URI")]
        public string? ExtraUrlText { get; set; }

        [JsonProperty("itemURL", NullValueHandling = NullValueHandling.Ignore)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1056:Uri properties should not be strings", Justification = "SaleFinder does not output full URIs for this property")]
        public string? ItemUrl { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string? ItemName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public long? SkuCount { get; set; }

        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
    }

    public enum Shape { Rectangle };

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated by JSON.NET using reflection")]
    internal class PageConverter : JsonConverter<Page?>
    {
        public override Page? ReadJson(JsonReader reader, Type objectType, Page? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;

            // Deserialise the entire JSON object into a JObject
            var pageObject = JObject.Load(reader);

            // Create a Page instance and populate the standard (non-numeric) property names from the JSON
            var page = pageObject.ToObject<Page>(serializer);

            // Add the numeric property names to the Items collection
            foreach (var property in pageObject.Properties())
            {
                if (int.TryParse(property.Name, out var index))
                {
                    page.Items.Insert(index, property.Value.ToObject<Item>());
                }
            }

            return page;
        }

        public override void WriteJson(JsonWriter writer, Page? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            // Create a JObject and populate the standard (non-numeric) property names
            var pageObject = JObject.FromObject(value, serializer);

            // Add the values from the Items collection as numeric property name
            for (int i = 0; i < value.Items.Count; i++)
            {
                pageObject.Add(i.ToString("d", NumberFormatInfo.InvariantInfo), JObject.FromObject(value.Items[i]));
            }

            // Serialise the JObject to JSON
            pageObject.WriteTo(writer);
        }
    }
}