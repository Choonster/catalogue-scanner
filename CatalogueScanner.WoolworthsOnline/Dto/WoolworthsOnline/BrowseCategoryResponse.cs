﻿// Originally generated by quicktype (https://quicktype.io/), then manually cleaned up

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace CatalogueScanner.WoolworthsOnline.Dto.WoolworthsOnline
{
    public class BrowseCategoryResponse
    {
        public SeoMetaTags? SeoMetaTags { get; set; }

        [JsonInclude]
        public IEnumerable<Bundle> Bundles { get; internal set; } = new List<Bundle>();

        public long TotalRecordCount { get; set; }
        public object? UpperDynamicContent { get; set; }
        public object? LowerDynamicContent { get; set; }
        public RichRelevancePlacement? RichRelevancePlacement { get; set; }

        [JsonInclude]
        public IEnumerable<Aggregation> Aggregations { get; internal set; } = new List<Aggregation>();

        public bool HasRewardsCard { get; set; }
        public bool HasTobaccoItems { get; set; }
        public bool Success { get; set; }
    }

    public class Aggregation
    {
        public string? Name { get; set; }
        public string? DisplayName { get; set; }
        public string? Type { get; set; }
        public string? FilterType { get; set; }
        public string? FilterDataType { get; set; }
        public object? Results { get; set; }

        [JsonInclude]
        public IEnumerable<ResultsGrouped> ResultsGrouped { get; internal set; } = new List<ResultsGrouped>();

        public string? State { get; set; }
        public long Rank { get; set; }
        public bool AdditionalResults { get; set; }
        public string? DesignType { get; set; }
        public bool ShowFilter { get; set; }
        public object? Statement { get; set; }
        public bool DisplayCoachMarks { get; set; }
    }

    public class ResultsGrouped
    {
        public string? Alphabet { get; set; }

        [JsonInclude]
        public IEnumerable<Filter> Filters { get; internal set; } = new List<Filter>();
    }

    public class Filter
    {
        public string? Name { get; set; }
        public string? Term { get; set; }
        public ExtraOutputFields? ExtraOutputFields { get; set; }
        public object? Min { get; set; }
        public object? Max { get; set; }
        public bool Applied { get; set; }
        public long Count { get; set; }
        public object? Statement { get; set; }
        public bool DisplayCoachMarks { get; set; }
    }

    public class ExtraOutputFields
    {
    }

    public class Bundle
    {
        [JsonInclude] public IEnumerable<Product> Products { get; internal set; } = new List<Product>();
        public string? Name { get; set; }
        public string? DisplayName { get; set; }
    }

    public class Product
    {
        public long TileId { get; set; }
        public long Stockcode { get; set; }
        public string? Barcode { get; set; }
        public long GtinFormat { get; set; }
        public decimal? CupPrice { get; set; }
        public decimal? InstoreCupPrice { get; set; }
        public string? CupMeasure { get; set; }
        public string? CupString { get; set; }
        public string? InstoreCupString { get; set; }
        public bool HasCupPrice { get; set; }
        public bool InstoreHasCupPrice { get; set; }
        public decimal? Price { get; set; }
        public decimal? InstorePrice { get; set; }
        public string? Name { get; set; }
        public string? DisplayName { get; set; }

        [SuppressMessage("Design", "CA1056:URI-like properties should not be strings", Justification = "This property isn't a URI")]
        public string? UrlFriendlyName { get; set; }

        public string? Description { get; set; }
        public Uri? SmallImageFile { get; set; }
        public Uri? MediumImageFile { get; set; }
        public Uri? LargeImageFile { get; set; }
        public bool IsNew { get; set; }
        public bool IsOnSpecial { get; set; }
        public bool InstoreIsOnSpecial { get; set; }
        public bool IsEdrSpecial { get; set; }
        public decimal? SavingsAmount { get; set; }
        public decimal? InstoreSavingsAmount { get; set; }
        public decimal WasPrice { get; set; }
        public decimal InstoreWasPrice { get; set; }
        public long QuantityInTrolley { get; set; }
        public Unit Unit { get; set; }
        public double MinimumQuantity { get; set; }
        public bool HasBeenBoughtBefore { get; set; }
        public bool IsInTrolley { get; set; }
        public string? Source { get; set; }
        public long SupplyLimit { get; set; }
        public string? MaxSupplyLimitMessage { get; set; }
        public bool IsRanged { get; set; }
        public bool IsInStock { get; set; }
        public string? PackageSize { get; set; }
        public bool IsPmDelivery { get; set; }
        public bool IsForCollection { get; set; }
        public bool IsForDelivery { get; set; }
        public bool IsForExpress { get; set; }
        public object? ProductRestrictionMessage { get; set; }
        public object? ProductWarningMessage { get; set; }
        public Tag? CentreTag { get; set; }
        public bool IsCentreTag { get; set; }
        public Tag? ImageTag { get; set; }
        public HeaderTag? HeaderTag { get; set; }
        public bool HasHeaderTag { get; set; }
        public long UnitWeightInGrams { get; set; }
        public string? SupplyLimitMessage { get; set; }
        public string? SmallFormatDescription { get; set; }
        public string? FullDescription { get; set; }
        public bool IsAvailable { get; set; }
        public bool InstoreIsAvailable { get; set; }
        public bool IsPurchasable { get; set; }
        public bool InstoreIsPurchasable { get; set; }
        public bool AgeRestricted { get; set; }
        public double DisplayQuantity { get; set; }
        public object? RichDescription { get; set; }
        public bool IsDeliveryPass { get; set; }
        public bool HideWasSavedPrice { get; set; }
        public object? SapCategories { get; set; }
        public string? Brand { get; set; }
        public bool IsRestrictedByDeliveryMethod { get; set; }
        public Tag? FooterTag { get; set; }
        public bool IsFooterEnabled { get; set; }
        
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public long Diagnostics { get; set; }

        public bool IsBundle { get; set; }
        public bool IsInFamily { get; set; }
        public object? ChildProducts { get; set; }
        public object? UrlOverride { get; set; }

        [JsonInclude]
        public IDictionary<string, string> AdditionalAttributes { get; internal set; } = new Dictionary<string, string>();

        [JsonInclude]
        public IEnumerable<Uri> DetailsImagePaths { get; internal set; } = new List<Uri>();

        public string? Variety { get; set; }

        public Rating? Rating { get; set; }

        public bool HasProductSubs { get; set; }
        public bool IsSponsoredAd { get; set; }
        public object? AdId { get; set; }
        public object? AdIndex { get; set; }
        public bool IsMarketProduct { get; set; }
        public bool IsGiftable { get; set; }
        public object? Vendor { get; set; }
        public bool Untraceable { get; set; }
        public object? ThirdPartyProductInfo { get; set; }
        public object? MarketFeatures { get; set; }
        public object? MarketSpecifications { get; set; }
        public SupplyLimitSource SupplyLimitSource { get; set; }
        public object? Tags { get; set; }
        public bool IsPersonalisedByPurchaseHistory { get; set; }
        public bool IsFromFacetedSearch { get; set; }
        public DateTimeOffset? NextAvailabilityDate { get; set; }
    }

    public class Tag
    {
        public string? TagContent { get; set; }
        public string? TagLink { get; set; }
        public string? FallbackText { get; set; }
        public TagType TagType { get; set; }
        public MultibuyData? MultibuyData { get; set; }
        public string? TagContentText { get; set; }
        public object? DualImageTagContent { get; set; }
    }

    public class MultibuyData
    {
        public long Quantity { get; set; }
        public decimal Price { get; set; }
        public string? CupTag { get; set; }
    }

    public class HeaderTag
    {
        public string? BackgroundColor { get; set; }
        public string? BorderColor { get; set; }
        public string? TextColor { get; set; }
        public string? Content { get; set; }
        public object? TagLink { get; set; }
        public Promotion Promotion { get; set; }
    }

    public class RichRelevancePlacement
    {
        [JsonPropertyName("placement_name")]
        public object? PlacementName { get; set; }

        [JsonPropertyName("message")]
        public object? Message { get; set; }

        [JsonInclude]
        public IEnumerable<object> Products { get; internal set; } = new List<object>();

        [JsonInclude]
        public IEnumerable<object> Items { get; internal set; } = new List<object>();

        [JsonInclude]
        public IEnumerable<object> StockcodesForDiscover { get; internal set; } = new List<object>();
    }

    public class SeoMetaTags
    {
        public string? Title { get; set; }
        public string? MetaDescription { get; set; }

        [JsonInclude]
        public IEnumerable<object> Groups { get; internal set; } = new List<object>();
    }

    public class Rating
    {
        public double Average { get; set; }
        public long FiveStarCount { get; set; }
        public double FiveStarPercentage { get; set; }
        public long FourStarCount { get; set; }
        public double FourStarPercentage { get; set; }
        public long OneStarCount { get; set; }
        public double OneStarPercentage { get; set; }
        public long RatingCount { get; set; }
        public long RatingSum { get; set; }
        public long ReviewCount { get; set; }
        public long ThreeStarCount { get; set; }
        public double ThreeStarPercentage { get; set; }
        public long TwoStarCount { get; set; }
        public double TwoStarPercentage { get; set; }
    }

    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum TagType
    {
        Html,
        None,
    }

    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum Promotion
    {
        Special,
        PriceDropped,
        LowPrice
    }

    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum SupplyLimitSource
    {
        ProductLimit
    }

    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum Unit
    {
        Each,
        KG
    }
}
