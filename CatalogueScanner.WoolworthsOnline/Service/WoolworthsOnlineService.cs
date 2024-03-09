using CatalogueScanner.Core.Http;
using CatalogueScanner.Core.Utility;
using CatalogueScanner.WoolworthsOnline.Dto.WoolworthsOnline;
using Microsoft.Net.Http.Headers;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization.Metadata;

namespace CatalogueScanner.WoolworthsOnline.Service;

public class WoolworthsOnlineService(HttpClient httpClient)
{
    /// <summary>
    /// The maximum value for <see cref="BrowseCategoryRequest.PageSize"/>.
    /// </summary>
    public const int MaxBrowseCategoryDataPageSize = 36;

    private const string WoolworthsBaseUrl = "https://www.woolworths.com.au/";

    private readonly HttpClient httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

    private Uri BaseAddress => httpClient.BaseAddress ?? throw new InvalidOperationException("httpClient.BaseAddress is null");

    /// <summary>
    /// The time of week when Coles Online changes its specials.
    /// </summary>
    public static TimeOfWeek SpecialsResetTime => new(TimeSpan.Zero, DayOfWeek.Wednesday, "AUS Eastern Standard Time");

    public static Uri ProductUrlTemplate => new($"{WoolworthsBaseUrl}/shop/productdetails/[stockCode]");

    public async Task<CookieCollection> GetCookiesAsync(CancellationToken cancellationToken = default)
    {
        var url = BaseAddress;

        var response = await httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("Root response is null");

        if (!response.Headers.TryGetValues(HeaderNames.SetCookie, out var cookieHeaders))
        {
            return [];
        }

        var cookies = new CookieContainer();

        foreach (var header in cookieHeaders)
        {
            cookies.SetCookies(url, header);
        }

        return cookies.GetAllCookies();
    }

    public async Task<GetPiesCategoriesResponse> GetPiesCategoriesWithSpecialsAsync(CookieCollection cookies, CancellationToken cancellationToken = default)
    {
        var response = await GetAsync(
            "PiesCategoriesWithSpecials",
            WoolworthsOnlineSerializerContext.Default.GetPiesCategoriesResponse,
            cookies,
            cancellationToken
        ).ConfigureAwait(false) ?? throw new InvalidOperationException("PiesCategoriesWithSpecials response is null");
        return response;
    }

    public async Task<BrowseCategoryResponse> GetBrowseCategoryDataAsync(BrowseCategoryRequest request, CookieCollection cookies, CancellationToken cancellationToken = default)
    {
        #region null checks
        ArgumentNullException.ThrowIfNull(request);
        #endregion

        var response = await PostAsync(
            "browse/category",
            request,
            WoolworthsOnlineSerializerContext.Default.BrowseCategoryRequest,
            WoolworthsOnlineSerializerContext.Default.BrowseCategoryResponse,
            cookies,
            cancellationToken
        ).ConfigureAwait(false) ?? throw new InvalidOperationException("Browse Category response is null");
        if (!response.Success)
        {
            throw new InvalidOperationException("Browse Category response is unsuccussful");
        }

        return response;
    }

    public async Task<int> GetCategoryPageCountAsync(string categoryId, int pageSize, CookieCollection cookies, CancellationToken cancellationToken = default)
    {
        // Ignore pageSize for this request as we don't actually want any data
        var response = await GetBrowseCategoryDataAsync(new BrowseCategoryRequest
        {
            CategoryId = categoryId,
            PageNumber = 1,
            PageSize = 1,
        }, cookies, cancellationToken).ConfigureAwait(false);

        return (int)(response.TotalRecordCount / pageSize + 1);
    }

    private async Task<TResponse?> GetAsync<TResponse>(string path, JsonTypeInfo<TResponse> responseTypeInfo, CookieCollection cookies, CancellationToken cancellationToken)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri(path, UriKind.Relative));

        AddCookieHeader(requestMessage, cookies);

        var response = await httpClient.SendAsync(requestMessage, cancellationToken).ConfigureAwait(false);

        await response.EnsureSuccessStatusCodeDetailedAsync().ConfigureAwait(false);

        return await response.Content.ReadFromJsonAsync(responseTypeInfo, cancellationToken).ConfigureAwait(false);
    }

    private async Task<TResponse?> PostAsync<TRequest, TResponse>(string path, TRequest request, JsonTypeInfo<TRequest> requestTypeInfo, JsonTypeInfo<TResponse> responseTypeInfo, CookieCollection cookies, CancellationToken cancellationToken)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, new Uri(path, UriKind.Relative))
        {
            Content = JsonContent.Create(request, requestTypeInfo),
        };

        AddCookieHeader(requestMessage, cookies);

        var response = await httpClient.SendAsync(requestMessage, cancellationToken).ConfigureAwait(false);

        await response.EnsureSuccessStatusCodeDetailedAsync().ConfigureAwait(false);

        return await response.Content.ReadFromJsonAsync(responseTypeInfo, cancellationToken).ConfigureAwait(false);
    }

    private void AddCookieHeader(HttpRequestMessage requestMessage, CookieCollection cookies)
    {
        var cookieContainer = new CookieContainer();
        cookieContainer.Add(cookies);

        var cookieHeader = cookieContainer.GetCookieHeader(BaseAddress);

        requestMessage.Headers.Add(HeaderNames.Cookie, cookieHeader);
    }
}
