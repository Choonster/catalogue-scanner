﻿// Originally generated by quicktype (https://quicktype.io/), then manually cleaned up

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CatalogueScanner.WebScraping.Dto.ColesOnline
{
    public partial class ColrsCatalogEntryList
    {
        public string? Type { get; set; }
        public Categories? Categories { get; set; }

        [JsonInclude]
        public IEnumerable<Facet> Facets { get; internal set; } = new List<Facet>();

        [JsonInclude]
        public IEnumerable<Product> Products { get; internal set; } = new List<Product>();

        public SearchInfo? SearchInfo { get; set; }
    }

    public partial class Categories
    {
        [JsonInclude]
        [JsonPropertyName("parentCatgroup_id_search")]
        public IDictionary<string, string> ParentCatgroupIdSearch { get; internal set; } = new Dictionary<string, string>();
    }

    public partial class Facet
    {
        [JsonInclude]
        public IEnumerable<FacetValue> Values { get; internal set; } = new List<FacetValue>();

        public string? Name { get; set; }
        public bool ShowEspot { get; set; }
    }

    public partial class FacetValue
    {
        public long Count { get; set; }
        public string? Label { get; set; }
        public string? Value { get; set; }
    }

    public partial class Product
    {
        public Price? Price { get; set; }
        public AttributesMap? AttributesMap { get; set; }
        public string? ShortDescription { get; set; }

        [JsonPropertyName("singleSKUCatalogEntryID")]
        public string? SingleSkuCatalogEntryId { get; set; }

        public string? Manufacturer { get; set; }
        public string? Name { get; set; }
        public string? PartNumber { get; set; }
        public string? SeoToken { get; set; }
        public string? Thumbnail { get; set; }

        [JsonPropertyName("uniqueID")]
        public string? UniqueId { get; set; }

        public string? PurchaseLimit { get; set; }
        public string? UnitPrice { get; set; }

        [JsonPropertyName("tickettype")]
        public TicketType TicketType { get; set; }

        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)]
        [JsonPropertyName("promo_min_qty")]
        public long? PromoMinQty { get; set; }

        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)]
        [JsonPropertyName("promo_reward")]
        public decimal? PromoReward { get; set; }

        [JsonPropertyName("promo_type")]
        public PromoType? PromoType { get; set; }

        [JsonPropertyName("promo_desc")]
        public string? PromoDesc { get; set; }

        [JsonPropertyName("promo_id")]
        public string? PromoId { get; set; }

        public bool? Available { get; set; }
        public string? Rating { get; set; }
    
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)]
        public long Reviews { get; set; }
    }

    public partial class AttributesMap
    {
        [JsonInclude]
        [JsonPropertyName("AVERAGESIZE")]
        public IEnumerable<string> AverageSize { get; internal set; } = new List<string>();

        [JsonInclude]
        [JsonPropertyName("DELIVERYRESTRICTIONS")]
        public IEnumerable<string> DeliveryRestrictions { get; internal set; } = new List<string>();

        [JsonInclude]
        [JsonPropertyName("ONLINESIZEDESCRIPTION")]
        public IEnumerable<string> OnlinesizeDescription { get; internal set; } = new List<string>();

        [JsonInclude]
        [JsonPropertyName("LIQUORAGERESTRICTIONFLAG")]
        public IEnumerable<string> LiquorAgeRestrictionFlag { get; internal set; } = new List<string>();

        [JsonInclude]
        [JsonPropertyName("PRIMARYAISLE")]
        public IEnumerable<string> PrimaryAisle { get; internal set; } = new List<string>();

        [JsonInclude]
        [JsonPropertyName("WEIGHTEDITEMINDICATOR")]
        public IEnumerable<string> WeightedItemIndicator { get; internal set; } = new List<string>();

        [JsonInclude]
        [JsonPropertyName("EXCLUDEFROMSUBSTITUTIONFLAG")]
        public IEnumerable<string> ExcludeFromSubstitutionFlag { get; internal set; } = new List<string>();

        [JsonInclude]
        [JsonPropertyName("TOBACCOAGERESTRICTIONFLAG")]
        public IEnumerable<string> TobaccoAgeRestrictionFlag { get; internal set; } = new List<string>();

        [JsonInclude]
        [JsonPropertyName("TYPE")]
        public IEnumerable<string> Type { get; internal set; } = new List<string>();

        [JsonInclude]
        [JsonPropertyName("DISPLAYREVIEWS")]
        public IEnumerable<string> Displayreviews { get; internal set; } = new List<string>();
    }

    public partial class Price
    {
        public double? ListPrice { get; set; }

        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)]
        public decimal OfferPrice { get; set; }

        [JsonPropertyName("isNaN")]
        public bool IsNaN { get; set; }

        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)]
        public long DollarValue { get; set; }

        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)]
        public long CentValue { get; set; }
    }

    public partial class SearchInfo
    {
        public long PageSize { get; set; }
        public string? SearchTerm { get; set; }
        public string? CategoryId { get; set; }
        public string? SearchType { get; set; }
        public string? Manufacturer { get; set; }
        public string? ProfileName { get; set; }
        public string? SearchSource { get; set; }
        public string? IntentSearchTerm { get; set; }
        public string? OriginalSearchTerm { get; set; }
        public string? MetaData { get; set; }
        public string? Currency { get; set; }
        public string? FilterTerm { get; set; }
        public string? FilterType { get; set; }
        public string? FilterFacet { get; set; }
        public string? MaxPrice { get; set; }
        public string? MinPrice { get; set; }
        public string? OrderBy { get; set; }
        public string? PhysicalStoreIds { get; set; }
        public string? AdvancedFacetList { get; set; }
        public string? PageView { get; set; }

        [JsonConverter(typeof(BooleanStringConverter))]
        public bool PersonaliseSearch { get; set; }

        [JsonConverter(typeof(BooleanStringConverter))]
        public bool PersonaliseSort { get; set; }

        public string? ResponseTemplate { get; set; }
        public long CurrentPage { get; set; }
        public long TotalCount { get; set; }
        public bool ShowAddAllResults { get; set; }
        public Params? Params { get; set; }
    }

    public partial class Params
    {
    }

    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum PromoType
    {
        MultibuyMultiSku,
        MultibuySingleSku
    }

    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum TicketType
    {
        [JsonPropertyName("M")] M,
        [JsonPropertyName("M_0")] MZero,
        [JsonPropertyName("M_1")] MOne,
        [JsonPropertyName("N_0")] NZero,
        [JsonPropertyName("S_0")] SZero,
        [JsonPropertyName("S_1")] SOne,
        [JsonPropertyName("S10_0")] STenZero,
        [JsonPropertyName("S15_0")] SFifteenZero,
        [JsonPropertyName("S15_1")] SFifteenOne,
        [JsonPropertyName("S20_0")] STwentyZero,
        [JsonPropertyName("S20_1")] STwentyOne,
        [JsonPropertyName("S25_0")] STwentyFiveZero,
        [JsonPropertyName("S30_0")] SThirtyZero,
        [JsonPropertyName("S30_1")] SThirtyOne,
        [JsonPropertyName("S40_0")] SFourtyZero,
        [JsonPropertyName("S50_0")] SFiftyZero,
        [JsonPropertyName("S501_1")] SFiftyOne,
        [JsonPropertyName("V")] V,
        [JsonPropertyName("W_0")] WZero,
        [JsonPropertyName("V_0")] VZero,
        [JsonPropertyName("X_0")] XZero
    }

    public class BooleanStringConverter : JsonConverter<bool>
    {
        private const string TrueString = "true";
        private const string FalseString = "false";

        public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            #region null checks
            if (typeToConvert is null)
            {
                throw new ArgumentNullException(nameof(typeToConvert));
            }

            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            #endregion

            return reader.TokenType switch
            {
                JsonTokenType.True => true,
                JsonTokenType.False => false,
                JsonTokenType.String => bool.Parse(reader.GetString()!),
                _ => throw new JsonException($"Error reading {typeof(bool).Name} from {typeof(Utf8JsonReader).Name}. Current item is not a boolean or string: {reader.TokenType}"),
            };
        }

        public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
        {
            #region null checks
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            #endregion

            writer.WriteStringValue(value ? TrueString : FalseString);
        }
    }
}
