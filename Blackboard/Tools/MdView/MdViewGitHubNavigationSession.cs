using Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown.Cache;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView;

/// <summary>Verified repository Markdown document at an exact source location.</summary>
/// <param name="Target">Canonical repository source and optional fragment.</param>
/// <param name="Result">Verified bytes and freshness status.</param>
public sealed record MdViewGitHubLinkedDocument(
    MdViewResolvedLink Target, HttpDocumentResult Result);

/// <summary>
/// Bounded GitHub Markdown navigation, using a resolved default branch,
/// a verified README path and exact-ref repository file retrieval.
/// </summary>
public sealed class MdViewGitHubNavigationSession
{
    /// <summary>Maximum Markdown bytes displayed in this interactive mode.</summary>
    public const long DefaultMaximumDocumentBytes = 1024 * 1024;

    private readonly GitHubRepositoryFileSource _source;
    private readonly long _maximumDocumentBytes;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private MdViewGitHubLinkedDocument _current;

    private MdViewGitHubNavigationSession(
        GitHubRepositoryFileSource source, GitHubReadmeLocation readme,
        MdViewGitHubLinkedDocument current, int capacity, long maximumDocumentBytes)
    {
        _source = source;
        _current = current;
        _maximumDocumentBytes = maximumDocumentBytes;
        Readme = readme;
        History = new MdViewNavigationHistory(capacity);
        History.Visit(current.Target);
    }

    /// <summary>Resolved README identity and metadata freshness.</summary>
    public GitHubReadmeLocation Readme { get; }

    /// <summary>Current verified source and document content.</summary>
    public MdViewGitHubLinkedDocument Current => _current;

    /// <summary>Bounded source and fragment Back/Forward history.</summary>
    public MdViewNavigationHistory History { get; }

    /// <summary>Discovers and opens the repository's actual default-branch README.</summary>
    /// <param name="source">Caller-owned GitHub file provider.</param>
    /// <param name="owner">GitHub owner.</param>
    /// <param name="repository">Repository name.</param>
    /// <param name="historyCapacity">Maximum retained locations.</param>
    /// <param name="maximumDocumentBytes">Maximum Markdown bytes per source.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Verified README navigation session.</returns>
    public static async Task<MdViewGitHubNavigationSession> OpenAsync(
        GitHubRepositoryFileSource source, string owner, string repository,
        int historyCapacity = MdViewNavigationHistory.DefaultCapacity,
        long maximumDocumentBytes = DefaultMaximumDocumentBytes,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (maximumDocumentBytes < 1)
            throw new ArgumentOutOfRangeException(nameof(maximumDocumentBytes));
        _ = new MdViewNavigationHistory(historyCapacity);
        cancellationToken.ThrowIfCancellationRequested();
        var readme = await source.ResolveReadmeAsync(
            owner, repository, cancellationToken).ConfigureAwait(false);
        var file = readme.File;
        var target = new MdViewResolvedLink(
            MdViewDocumentOrigin.FromGitHub(
                file.Owner, file.Repository, file.Reference, file.Path), null);
        var result = await FetchAsync(source, target, maximumDocumentBytes, cancellationToken)
            .ConfigureAwait(false);
        return new MdViewGitHubNavigationSession(
            source, readme, new MdViewGitHubLinkedDocument(target, result),
            historyCapacity, maximumDocumentBytes);
    }

    /// <summary>Follows a validated repository-relative Markdown link.</summary>
    /// <param name="href">Relative path or same-document anchor.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
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
            _current = new MdViewGitHubLinkedDocument(target, result);
            History.Visit(target);
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>Reopens the preceding history location when available.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the history position changed.</returns>
    public Task<bool> BackAsync(CancellationToken cancellationToken = default) =>
        MoveAsync(true, cancellationToken);

    /// <summary>Reopens the next history location when available.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the history position changed.</returns>
    public Task<bool> ForwardAsync(CancellationToken cancellationToken = default) =>
        MoveAsync(false, cancellationToken);

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
            _current = new MdViewGitHubLinkedDocument(target, result);
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
        GitHubRepositoryFileSource source, MdViewResolvedLink target,
        long maximumDocumentBytes, CancellationToken token)
    {
        var origin = target.Origin;
        if (origin.Kind != MdViewDocumentOriginKind.GitHub ||
            origin.Owner is null || origin.Repository is null || origin.Ref is null)
            throw new ArgumentException("Canonical GitHub source required.", nameof(target));
        var result = await source.OpenAsync(
            new GitHubRepositoryFile(
                origin.Owner, origin.Repository, origin.Ref, origin.Location), token)
            .ConfigureAwait(false);
        if (result.Document is null)
            throw new IOException(result.Warning ?? "GitHub Markdown file is unavailable.");
        if (result.Document.Length > maximumDocumentBytes)
            throw new InvalidDataException(
                "GitHub Markdown exceeds the interactive browsing byte limit.");
        return result;
    }
}
