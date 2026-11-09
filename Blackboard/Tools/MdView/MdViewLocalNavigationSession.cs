namespace Afrowave.Toolbox.Blackboard.Tools.MdView;

/// <summary>
/// Opt-in local Markdown navigation controller. Owns one validated snapshot
/// at a time and keeps a bounded path/fragment history, not file contents.
/// </summary>
/// <remarks>
/// Each transition validates and opens its destination before modifying
/// history. On failure the previous page and history remain usable.
/// This is not connected to the interactive mdview CLI yet.
/// </remarks>
public sealed class MdViewLocalNavigationSession : IAsyncDisposable
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly long _maximumSnapshotBytes;
    private MdViewLocalLinkedDocument _current;
    private bool _disposed;

    private MdViewLocalNavigationSession(
        MdViewLocalLinkedDocument current, int historyCapacity, long maximumSnapshotBytes)
    {
        _current = current;
        _maximumSnapshotBytes = maximumSnapshotBytes;
        History = new MdViewNavigationHistory(historyCapacity);
        History.Visit(current.Target);
    }

    /// <summary>Bounded location history; callers must not mutate it independently.</summary>
    public MdViewNavigationHistory History { get; }

    /// <summary>Currently open validated local Markdown document.</summary>
    public MdViewLocalLinkedDocument Current
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _current;
        }
    }

    /// <summary>
    /// Opens the initial local Markdown document in its permitted navigation root.
    /// </summary>
    /// <param name="origin">Initial local source and root.</param>
    /// <param name="historyCapacity">Maximum retained navigation locations.</param>
    /// <param name="maximumSnapshotBytes">Per-open temporary snapshot budget.</param>
    /// <param name="cancellationToken">Cancels initialization.</param>
    /// <returns>Owner of the current validated document and history.</returns>
    public static async Task<MdViewLocalNavigationSession> OpenAsync(
        MdViewDocumentOrigin origin,
        int historyCapacity = MdViewNavigationHistory.DefaultCapacity,
        long maximumSnapshotBytes = 16L * 1024 * 1024 * 1024,
        CancellationToken cancellationToken = default)
    {
        // Validate the cap before staging the first document.
        _ = new MdViewNavigationHistory(historyCapacity);
        var document = await MdViewLocalLinkLoader.OpenAsync(
            origin, "#", maximumSnapshotBytes, cancellationToken).ConfigureAwait(false);
        return new MdViewLocalNavigationSession(
            document, historyCapacity, maximumSnapshotBytes);
    }

    /// <summary>
    /// Follows a relative Markdown link, committing history only after
    /// the destination has been validated and opened.
    /// </summary>
    /// <param name="href">Relative Markdown path or same-document fragment.</param>
    /// <param name="cancellationToken">Cancels navigation.</param>
    public async Task FollowAsync(string href, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            var target = MdViewLinkResolver.Resolve(_current.Target.Origin, href);
            if (target == History.Current)
                return;
            var opened = await MdViewLocalLinkLoader.OpenAsync(
                _current.Target.Origin, href, _maximumSnapshotBytes, cancellationToken)
                .ConfigureAwait(false);
            await ReplaceAsync(opened, () => History.Visit(opened.Target)).ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>Opens and navigates to the previous location if available.</summary>
    /// <param name="cancellationToken">Cancels navigation.</param>
    /// <returns>False if there is no previous location.</returns>
    public Task<bool> BackAsync(CancellationToken cancellationToken = default) =>
        MoveAsync(back: true, cancellationToken);

    /// <summary>Opens and navigates to the next location if available.</summary>
    /// <param name="cancellationToken">Cancels navigation.</param>
    /// <returns>False if there is no next location.</returns>
    public Task<bool> ForwardAsync(CancellationToken cancellationToken = default) =>
        MoveAsync(back: false, cancellationToken);

    private async Task<bool> MoveAsync(bool back, CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            var target = back ? History.BackTarget : History.ForwardTarget;
            if (target is null)
                return false;

            var current = _current.Target.Origin;
            var relative = Path.GetRelativePath(
                Path.GetDirectoryName(current.Location)!, target.Origin.Location)
                .Replace(Path.DirectorySeparatorChar, '/');
            if (target.Fragment is not null)
                relative += "#" + target.Fragment;
            var opened = await MdViewLocalLinkLoader.OpenAsync(
                current, relative, _maximumSnapshotBytes, cancellationToken)
                .ConfigureAwait(false);
            if (opened.Target != target)
            {
                await opened.DisposeAsync().ConfigureAwait(false);
                throw new InvalidDataException("History location no longer matches the target.");
            }

            await ReplaceAsync(opened, () =>
            {
                if (back)
                    History.GoBack();
                else
                    History.GoForward();
            }).ConfigureAwait(false);
            return true;
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task ReplaceAsync(MdViewLocalLinkedDocument opened, Action updateHistory)
    {
        // No potentially failing IO occurs after history is committed.
        // Dispose the old snapshot before publishing the new current source.
        // A failure disposing the previous copy cannot invalidate the newly
        // validated destination, but must still be reported to the caller.
        var previous = _current;
        updateHistory();
        _current = opened;
        await previous.DisposeAsync().ConfigureAwait(false);
    }

    /// <summary>Disposes the currently open snapshot and session synchronization.</summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;
        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            if (_disposed)
                return;
            _disposed = true;
            await _current.DisposeAsync().ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }
}
