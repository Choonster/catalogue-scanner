using DotNext.Threading;
using Microsoft.Playwright;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace CatalogueScanner.WebScraping.Service;

/// <summary>
/// <para>Manages Playwright browser instances and contexts for Durable Functions.</para>
/// <para>Each durable orchestration ID receives its own <see cref="IBrowser"/> instance. Activity functions within an orchestration share the same <see cref="IBrowser"/> instance but create their own <see cref="IBrowserContext"/> instances from it.</para>
/// </summary>
public class PlaywrightBrowserManager : IAsyncDisposable
{
    private readonly AsyncLazy<IPlaywright> playwright = new(async (_) => await Playwright.CreateAsync());
    private readonly ConcurrentDictionary<string, AsyncLazy<IBrowser>> browsersByInstanceId = new();

    /// <summary>
    /// Gets the <see cref="IPlaywright"/> instance.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The <see cref="IPlaywright"/> instance</returns>
    public async Task<IPlaywright> GetPlaywrightAsync(CancellationToken cancellationToken) => await playwright.WithCancellation(cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Creates a new <see cref="IBrowserContext"/> from the browser with the specified <paramref name="instanceId"/>, launching the browser if it hasn't already been launched.
    /// </summary>
    /// <param name="instanceId">The instance ID of the browser to create the context from.</param>
    /// <param name="browserType">The browser type to launch. Defaults to <see cref="IPlaywright.Chromium"/>. Only used if the browser hasn't already been launched.</param>
    /// <param name="browserTypeLaunchOptions">The options to launch the browser with. Only used if the browser hasn't already been launched.</param>
    /// <param name="browserNewContextOptions">The options to create the context with.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The <see cref="IBrowserContext"/> instance</returns>
    public async Task<IBrowserContext> NewContextAsync(
        string instanceId,
        IBrowserType? browserType = null,
        BrowserTypeLaunchOptions? browserTypeLaunchOptions = null,
        BrowserNewContextOptions? browserNewContextOptions = null,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var playwright = await GetPlaywrightAsync(cancellationToken).ConfigureAwait(false);

        var browser = await browsersByInstanceId.GetOrAdd(
            instanceId,
            (instanceId) => new AsyncLazy<IBrowser>(async (_) => await (browserType ?? playwright.Chromium).LaunchAsync(browserTypeLaunchOptions).ConfigureAwait(false))
        )
            .WithCancellation(cancellationToken)
            .ConfigureAwait(false);

        using (await browser.AcquireLockAsync(cancellationToken).ConfigureAwait(false))
        {
            return await browser.NewContextAsync(browserNewContextOptions).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Closes the Playwright browser with the specified <paramref name="instanceId"/>, if it exists.
    /// </summary>
    /// <param name="instanceId">The instance ID of the browser to close</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns> <see langword="true"/> if the browser existed and was closed successfully; otherwise, <see langword="false"/></returns>
    public async Task<bool> CloseBrowserAsync(string instanceId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!browsersByInstanceId.TryRemove(instanceId, out var browserLazy))
        {
            return false;
        }

        if (!(browserLazy.Value?.TryGet(out var browser) ?? false) || browser is null)
        {
            return false;
        }

        using (await browser.AcquireLockAsync(cancellationToken).ConfigureAwait(false))
        {
            await browser.CloseAsync().ConfigureAwait(false);
            return true;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);

        GC.SuppressFinalize(this);
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        foreach (var browserLazy in browsersByInstanceId.Values)
        {
            if (browserLazy.Value?.TryGet(out var browser) ?? false)
            {
                await browser.DisposeAsync().ConfigureAwait(false);
            }
        }

        browsersByInstanceId.Clear();

        var playwright = await this.playwright.WithCancellation(CancellationToken.None).ConfigureAwait(false);
        playwright.Dispose();
    }
}
