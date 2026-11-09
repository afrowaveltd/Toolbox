using Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown.Cache;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView;

/// <summary>A verified HTTPS document and its precise navigation location.</summary>
/// <param name="Target">Current document origin and heading fragment.</param>
/// <param name="Result">Verified cache result and explicitly reported freshness state.</param>
public sealed record MdViewHttpsLinkedDocument(
    MdViewResolvedLink Target, HttpDocumentResult Result);

/// <summary>
/// Opt-in HTTPS Markdown navigation. Uses the existing verified HTTP/cache
/// adapter, retains only one current document and bounded location history.
/// </summary>
/// <remarks>
/// All target paths must stay on the original HTTPS origin. Each changed
/// document is fetched/revalidated before history is changed. Same-document
/// anchor jumps reuse already validated content. Caller owns the source's
/// HttpClient and cache, including its no-automatic-redirect policy.
/// </remarks>
public sealed class MdViewHttpsNavigationSession
{
    /// <summary>Maximum single-document UTF-8 bytes for whole-render browsing.</summary>
    public const long DefaultMaximumDocumentBytes = 1024 * 1024;

    private readonly HttpMarkdownDocumentSource _source;
    private readonly long _maximumDocumentBytes;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private MdViewHttpsLinkedDocument _current;

    private MdViewHttpsNavigationSession(
        HttpMarkdownDocumentSource source, MdViewHttpsLinkedDocument current,
        int historyCapacity, long maximumDocumentBytes)
    {
        _source = source;
        _current = current;
        _maximumDocumentBytes = maximumDocumentBytes;
        History = new MdViewNavigationHistory(historyCapacity);
        History.Visit(current.Target);
    }

    /// <summary>The current verified Markdown content and its freshness state.</summary>
    public MdViewHttpsLinkedDocument Current => _current;

    /// <summary>Bounded Back/Forward history of URLs and fragments.</summary>
    public MdViewNavigationHistory History { get; }

    /// <summary>Opens the first HTTPS source through the verified cache.</summary>
    /// <param name="source">Caller-configured HTTPS document adapter.</param>
    /// <param name="origin">Absolute HTTPS source identity.</param>
    /// <param name="historyCapacity">Maximum retained history entries.</param>
    /// <param name="maximumDocumentBytes">Maximum source bytes displayed in browser.</param>
    /// <param name="cancellationToken">Cancellation of source retrieval.</param>
    /// <returns>A navigation session rooted at the verified source.</returns>
    public static async Task<MdViewHttpsNavigationSession> OpenAsync(
        HttpMarkdownDocumentSource source, MdViewDocumentOrigin origin,
        int historyCapacity = MdViewNavigationHistory.DefaultCapacity,
        long maximumDocumentBytes = DefaultMaximumDocumentBytes,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(origin);
        if (origin.Kind != MdViewDocumentOriginKind.Https)
            throw new ArgumentException("An HTTPS document origin is required.", nameof(origin));
        if (maximumDocumentBytes < 1)
            throw new ArgumentOutOfRangeException(nameof(maximumDocumentBytes));
        _ = new MdViewNavigationHistory(historyCapacity);
        cancellationToken.ThrowIfCancellationRequested();

        var target = new MdViewResolvedLink(origin, null);
        var result = await FetchAsync(source, target, maximumDocumentBytes, cancellationToken)
            .ConfigureAwait(false);
        return new MdViewHttpsNavigationSession(
            source, new MdViewHttpsLinkedDocument(target, result),
            historyCapacity, maximumDocumentBytes);
    }

    /// <summary>
    /// Follows a same-origin Markdown link after verifying its destination.
    /// A failed fetch leaves the current page and history unchanged.
    /// </summary>
    /// <param name="href">Relative path or heading anchor.</param>
    /// <param name="cancellationToken">Cancels navigation.</param>
    public async Task FollowAsync(string href, CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var target = MdViewLinkResolver.Resolve(_current.Target.Origin, href);
            if (target == History.Current)
                return;
            var result = target.Origin == _current.Target.Origin
                ? _current.Result
                : await FetchAsync(_source, target, _maximumDocumentBytes, cancellationToken)
                    .ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            _current = new MdViewHttpsLinkedDocument(target, result);
            History.Visit(target);
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>Moves backward after revalidating a changed remote source.</summary>
    /// <param name="cancellationToken">Cancels navigation.</param>
    /// <returns>False when there is no preceding location.</returns>
    public Task<bool> BackAsync(CancellationToken cancellationToken = default) =>
        MoveAsync(back: true, cancellationToken);

    /// <summary>Moves forward after revalidating a changed remote source.</summary>
    /// <param name="cancellationToken">Cancels navigation.</param>
    /// <returns>False when there is no following location.</returns>
    public Task<bool> ForwardAsync(CancellationToken cancellationToken = default) =>
        MoveAsync(back: false, cancellationToken);

    private async Task<bool> MoveAsync(bool back, CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var target = back ? History.BackTarget : History.ForwardTarget;
            if (target is null)
                return false;
            var result = target.Origin == _current.Target.Origin
                ? _current.Result
                : await FetchAsync(_source, target, _maximumDocumentBytes, cancellationToken)
                    .ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            _current = new MdViewHttpsLinkedDocument(target, result);
            if (back)
                History.GoBack();
            else
                History.GoForward();
            return true;
        }
        finally
        {
            _gate.Release();
        }
    }

    private static async Task<HttpDocumentResult> FetchAsync(
        HttpMarkdownDocumentSource source, MdViewResolvedLink target,
        long maximumDocumentBytes, CancellationToken token)
    {
        var result = await source.OpenAsync(
            new Uri(target.Origin.Location, UriKind.Absolute), token).ConfigureAwait(false);
        if (result.Document is null)
            throw new IOException(result.Warning ?? "HTTPS Markdown document is unavailable.");
        if (result.Document.Length > maximumDocumentBytes)
            throw new InvalidDataException(
                "HTTPS Markdown document exceeds the interactive browsing byte limit.");
        return result;
    }
}
