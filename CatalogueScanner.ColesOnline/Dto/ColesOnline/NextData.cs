﻿// Originally generated by quicktype (https://quicktype.io/), then manually cleaned up

using System.Text.Json.Serialization;

namespace CatalogueScanner.ColesOnline.Dto.ColesOnline;

public class NextData
{
    public NextDataProps? Props { get; set; }
    public string? Page { get; set; }
    public object? Query { get; set; }
    public string? BuildId { get; set; }
    public RuntimeConfig? RuntimeConfig { get; set; }
    public bool IsFallback { get; set; }
    public bool Gssp { get; set; }
    public bool AppGip { get; set; }
    public string? Locale { get; set; }
    public IEnumerable<string> Locales { get; } = [];
    public string? DefaultLocale { get; set; }
}

public class NextDataProps
{
    public NextDataPageProps? PageProps { get; set; }
}

public class NextDataPageProps
{
    public long? InitStoreId { get; set; }
    public Uri? AssetsUrl { get; set; }
    public NextDataPagePropsData? Data { get; set; }
    public object? Error { get; set; }
    public Uri? ResolvedUrl { get; set; }
    public bool IndexFollowFlag { get; set; }
    public InitialState? InitialState { get; set; }
}

public class NextDataPagePropsData
{
    public long LastModifiedDate { get; set; }
    public string? TemplateName { get; set; }
    public string? DesignPath { get; set; }
    public string? CssClassNames { get; set; }
    public string? BrandSlug { get; set; }
    public string? Title { get; set; }
    public string? Language { get; set; }
    public string? Type { get; set; }
    public IEnumerable<string> ItemsOrder { get; } = [];
    public string? Path { get; set; }
    public string? HierarchyType { get; set; }
}

public class InitialState
{
    public UserState? User { get; set; }
    public DrawerState? Modal { get; set; }
    public NotificationsState? Notifications { get; set; }
    public MpgsState? Mpgs { get; set; }
    public CheckoutState? Checkout { get; set; }
    public TrolleyState? Trolley { get; set; }
    public DrawerState? Drawer { get; set; }
    public ShoppingMethodState? ShoppingMethod { get; set; }
    public EnquiryFormsState? EnquiryForms { get; set; }
    public ListState? List { get; set; }
    public ContentState? Content { get; set; }
    public ApiState? BffApi { get; set; }
    public ApiState? AemApi { get; set; }
    public ApiState? EnquiryFormApi { get; set; }
}

public class ApiState
{
    public IDictionary<string, object> Queries { get; } = new Dictionary<string, object>();
    public IDictionary<string, object> Mutations { get; } = new Dictionary<string, object>();
    public IDictionary<string, object> Provided { get; } = new Dictionary<string, object>();
    public IDictionary<string, object> Subscriptions { get; } = new Dictionary<string, object>();
    public ApiConfig? Config { get; set; }
}

public class ApiConfig
{
    public bool Online { get; set; }
    public bool Focused { get; set; }
    public bool MiddlewareRegistered { get; set; }
    public bool RefetchOnFocus { get; set; }
    public bool RefetchOnReconnect { get; set; }
    public bool RefetchOnMountOrArgChange { get; set; }
    public long KeepUnusedDataFor { get; set; }
    public string? ReducerPath { get; set; }
}

public class CheckoutState
{
    public object? PaymentMethod { get; set; }
}

public class ContentState
{
    public string? PageCategory { get; set; }
    public string? PageSubCategory { get; set; }
    public bool DisplayFilter { get; set; }
    public IEnumerable<object> ExpandFilter { get; } = [];
    public bool NextLevel { get; set; }
}

public class DrawerState
{
    public IEnumerable<object> Active { get; } = [];
    public object? State { get; set; }
}

public class EnquiryFormsState
{
    public IEnumerable<object> Ids { get; } = [];
    public object? Entities { get; set; }
}

public class ListState
{
    public object? Error { get; set; }
    public IEnumerable<object> PatchListItemsQueue { get; } = [];
}

public class MpgsState
{
    public FormFieldValidity? FormFieldValidity { get; set; }
    public string? InitStatus { get; set; }
    public string? SubmitStatus { get; set; }
    public object? SuccessData { get; set; }
    public bool UnexpectedError { get; set; }
    public bool SaveToProfile { get; set; }
}

public class FormFieldValidity
{
    public string? CardNumberValidity { get; set; }
    public string? ExpiryYearValidity { get; set; }
    public string? ExpiryMonthValidity { get; set; }
    public string? CvvValidity { get; set; }
}

public class NotificationsState
{
    public IEnumerable<object> NotificationsNotifications { get; } = [];
}

public class ShoppingMethodState
{
    public bool IsEditing { get; set; }
    public bool DidStoreIdChange { get; set; }
    public object? State { get; set; }
}

public class TrolleyState
{
    public object? Error { get; set; }
    public IEnumerable<object> ItemsBeingUpdated { get; } = [];
    public string? StoreId { get; set; }
    public TrolleyValidation? Validation { get; set; }
    public IEnumerable<object> UpdateQueue { get; } = [];
}

public class TrolleyValidation
{
    public bool IsValidating { get; set; }
    public bool IsValid { get; set; }
    public object? ValidationErrors { get; set; }
    public object? Error { get; set; }
    public object? RestrictedItems { get; set; }
}

public class UserState
{
    public object? Error { get; set; }
    public UserAuth? Auth { get; set; }
}

public class UserAuth
{
    public bool Authenticated { get; set; }
}

public class RuntimeConfig
{
    [JsonPropertyName("AEM_IMAGE_URL")]
    public Uri? AemImageUrl { get; set; }

    [JsonPropertyName("ADOBE_LAUNCH_ANALYTICS_URL")]
    public Uri? AdobeLaunchAnalyticsUrl { get; set; }

    [JsonPropertyName("ADOBE_LAUNCH_ANALYTICS_ID")]
    public string? AdobeLaunchAnalyticsId { get; set; }

    [JsonPropertyName("APP_INSIGHTS_INSTRUMENTATION_KEY")]
    public Guid AppInsightsInstrumentationKey { get; set; }

    [JsonPropertyName("BFF_API_SUBSCRIPTION_KEY")]
    public string? BffApiSubscriptionKey { get; set; }

    [JsonPropertyName("CCP_PROFILE_MANAGEMENT_URL")]
    public Uri? CcpProfileManagementUrl { get; set; }

    [JsonPropertyName("COL_URL")]
    public Uri? ColUrl { get; set; }

    [JsonPropertyName("COL4BUSINESS_URL")]
    public Uri? Col4BusinessUrl { get; set; }

    [JsonPropertyName("CITRUS_AD_IMAGE_URL")]
    public Uri? CitrusAdImageUrl { get; set; }

    [JsonPropertyName("CITRUS_HOST_URL")]
    public Uri? CitrusHostUrl { get; set; }

    [JsonPropertyName("CITRUS_REFERER_URL")]
    public Uri? CitrusRefererUrl { get; set; }

    [JsonPropertyName("CV_CHATBOT_URL")]
    public Uri? CvChatbotUrl { get; set; }

    [JsonPropertyName("COLES_BFF_API_URL")]
    public Uri? ColesBffApiUrl { get; set; }

    [JsonPropertyName("COLES_ENV")]
    public string? ColesEnv { get; set; }

    [JsonPropertyName("PRODUCT_IMAGE_URL")]
    public Uri? ProductImageUrl { get; set; }

    [JsonPropertyName("COLES_MPGS_URL")]
    public Uri? ColesMpgsUrl { get; set; }

    [JsonPropertyName("LD_SDK_CLIENT_ID")]
    public string? LdSdkClientId { get; set; }

    [JsonPropertyName("AEM_HOST_URL")]
    public Uri? AemHostUrl { get; set; }

    [JsonPropertyName("AEM_ROOT_PATH")]
    public string? AemRootPath { get; set; }

    [JsonPropertyName("AEM_XF_ROOT_PATH")]
    public string? AemXfRootPath { get; set; }

    [JsonPropertyName("NEXT_HOST_URL")]
    public Uri? NextHostUrl { get; set; }

    [JsonPropertyName("GOOGLE_RECAPTCHA_V3_KEY")]
    public string? GoogleRecaptchaV3Key { get; set; }

    [JsonPropertyName("SENTRY_TRACE_SAMPLE_RATE")]
    public string? SentryTraceSampleRate { get; set; }
}
