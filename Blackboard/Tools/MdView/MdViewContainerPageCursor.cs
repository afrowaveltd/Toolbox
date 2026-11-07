using Afrowave.Toolbox.Blackboard.TextFiles;
using Afrowave.Toolbox.ExchangeHouse.Markdown;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView;

/// <summary>
/// Bounded forward-only Markdown container cursor over a validated snapshot.
/// Forward pages reuse scanner state; backward navigation replays from start.
/// </summary>
/// <remarks>
/// Does not buffer complete documents or claim constant-time backward jumps.
/// Source lines and fence ownership come from the existing incremental scanner.
/// </remarks>
public sealed class MdViewContainerPageCursor : IAsyncDisposable
{
    private readonly Utf8ValidatedSnapshot _snapshot;
    private StreamReader? _reader;
    private IAsyncEnumerator<MarkdownContainerLine>? _scanner;
    private long _nextLine;
    private bool _disposed;

    /// <summary>Creates a cursor; the caller retains ownership of the snapshot.</summary>
    /// <param name="snapshot">An already validated, live UTF-8 snapshot.</param>
    public MdViewContainerPageCursor(Utf8ValidatedSnapshot snapshot) =>
        _snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));

    /// <summary>Reads at most 128 classified physical lines, replaying only for backward navigation.</summary>
    /// <param name="firstLine">Zero-based first physical line.</param>
    /// <param name="maxLines">Requested line count, from 1 to 128.</param>
    /// <param name="cancellationToken">Cancellation for scanning and replay.</param>
    /// <returns>Bounded source-preserving line metadata.</returns>
    public async Task<IReadOnlyList<MarkdownContainerLine>> ReadAsync(
        long firstLine, int maxLines, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (firstLine < 0)
            throw new ArgumentOutOfRangeException(nameof(firstLine));
        if (maxLines is < 1 or > 128)
            throw new ArgumentOutOfRangeException(nameof(maxLines));

        if (_scanner is null || firstLine < _nextLine)
            await ResetAsync(cancellationToken).ConfigureAwait(false);

        while (_nextLine < firstLine)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!await _scanner!.MoveNextAsync().ConfigureAwait(false))
                return [];
            _nextLine++;
        }

        var result = new List<MarkdownContainerLine>(maxLines);
        while (result.Count < maxLines)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!await _scanner!.MoveNextAsync().ConfigureAwait(false))
                break;
            result.Add(_scanner.Current);
            _nextLine++;
        }
        return result;
    }

    private async Task ResetAsync(CancellationToken cancellationToken)
    {
        if (_scanner is not null)
        {
            await _scanner.DisposeAsync().ConfigureAwait(false);
            _scanner = null;
        }
        _reader?.Dispose();
        _reader = _snapshot.OpenReader();
        _scanner = MarkdownIncrementalContainerScanner.ScanAsync(
            _reader, cancellationToken: cancellationToken)
            .GetAsyncEnumerator(cancellationToken);
        _nextLine = 0;
    }

    /// <summary>Closes this scanner and reader without disposing the caller's snapshot.</summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;
        _disposed = true;
        if (_scanner is not null)
            await _scanner.DisposeAsync().ConfigureAwait(false);
        _reader?.Dispose();
    }
}
