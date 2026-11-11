using System.Text;
using Afrowave.Toolbox.Blackboard.TextFiles;
using Afrowave.Toolbox.ExchangeHouse.Markdown;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView;

/// <summary>Types of source coordinates stored in the disk-backed Markdown index.</summary>
public enum MdViewIndexedItemKind
{
    /// <summary>Recognized top-level ATX heading outside fenced code.</summary>
    Heading = 1,
    /// <summary>Inline Markdown link outside fenced code.</summary>
    Link = 2
}

/// <summary>One source navigation result; no document or AST is retained.</summary>
/// <param name="Kind">Heading or link.</param>
/// <param name="LineNumber">Zero-based physical line in the validated snapshot.</param>
/// <param name="Utf16Offset">Zero-based decoded document UTF-16 source offset.</param>
/// <param name="Label">Bounded heading title or link label.</param>
/// <param name="Destination">Original inline link destination, or null for a heading.</param>
public sealed record MdViewIndexedNavigationItem(
    MdViewIndexedItemKind Kind, long LineNumber, long Utf16Offset,
    string Label, string? Destination);

/// <summary>
/// Source-bound, disk-backed heading/link catalog built from one sequential
/// scan of a fully validated private UTF-8 snapshot.
/// </summary>
/// <remarks>
/// Three temporary files hold bounded records and per-kind 64-bit offsets.
/// The caller owns the snapshot; the index owns and deletes only its temp
/// files. No full Markdown document, link list or heading list is retained
/// in RAM. This conservative first pass recognizes ATX headings and inline
/// links on individual lines, excluding fenced code, but not reference links
/// or links split across physical lines. Indexed links are source positions,
/// not automatically approved navigation destinations.
/// </remarks>
public sealed class MdViewDiskNavigationIndex : IAsyncDisposable
{
    /// <summary>Combined default maximum temporary record/offset storage: 128 MiB.</summary>
    public const long DefaultMaximumIndexBytes = 128L * 1024 * 1024;
    /// <summary>Maximum heading or link label length stored in UTF-16 code units.</summary>
    public const int MaximumLabelCharacters = 160;
    /// <summary>Maximum eligible inline link destination length in UTF-16 units.</summary>
    public const int MaximumDestinationCharacters = 2048;
    /// <summary>Maximum number of results returned by a single disk read.</summary>
    public const int MaximumPageItems = 128;
    private const int MaximumRecordBytes = 16 * 1024;

    private readonly FileStream _records;
    private readonly FileStream _headings;
    private readonly FileStream _links;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private bool _disposed;

    private MdViewDiskNavigationIndex(
        FileStream records, FileStream headings, FileStream links,
        long headingCount, long linkCount, long sourceLineCount)
    {
        _records = records;
        _headings = headings;
        _links = links;
        HeadingCount = headingCount;
        LinkCount = linkCount;
        SourceLineCount = sourceLineCount;
    }

    /// <summary>Number of indexed heading locations.</summary>
    public long HeadingCount { get; }
    /// <summary>Number of indexed inline link locations.</summary>
    public long LinkCount { get; }
    /// <summary>Number of source lines scanned from the validated snapshot.</summary>
    public long SourceLineCount { get; }
    /// <summary>Disk bytes occupied by records and both offset tables.</summary>
    public long IndexBytes => checked(_records.Length + _headings.Length + _links.Length);

    /// <summary>Builds a bounded disk index without loading the source document into RAM.</summary>
    /// <param name="snapshot">Completed, caller-owned validated UTF-8 snapshot.</param>
    /// <param name="maximumIndexBytes">Combined disk budget including both offset tables.</param>
    /// <param name="cancellationToken">Cancels scanning, deletes partial indexes.</param>
    /// <returns>Snapshot-local index that must be disposed before its snapshot.</returns>
    public static async Task<MdViewDiskNavigationIndex> CreateAsync(
        Utf8ValidatedSnapshot snapshot,
        long maximumIndexBytes = DefaultMaximumIndexBytes,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        if (maximumIndexBytes < 64)
            throw new ArgumentOutOfRangeException(nameof(maximumIndexBytes));
        cancellationToken.ThrowIfCancellationRequested();

        var records = CreateTemp();
        FileStream? headings = null;
        FileStream? links = null;
        try
        {
            headings = CreateTemp();
            links = CreateTemp();
            long headingCount = 0;
            long linkCount = 0;
            long scanned = 0;

            using var recordWriter = new BinaryWriter(records, Encoding.UTF8, leaveOpen: true);
            using var headingWriter = new BinaryWriter(headings, Encoding.UTF8, leaveOpen: true);
            using var linkWriter = new BinaryWriter(links, Encoding.UTF8, leaveOpen: true);

            void Store(MdViewIndexedNavigationItem item)
            {
                cancellationToken.ThrowIfCancellationRequested();
                using var buffer = new MemoryStream();
                using (var payload = new BinaryWriter(buffer, Encoding.UTF8, leaveOpen: true))
                {
                    payload.Write((byte)item.Kind);
                    payload.Write(item.LineNumber);
                    payload.Write(item.Utf16Offset);
                    payload.Write(item.Label);
                    payload.Write(item.Destination ?? "");
                }
                if (buffer.Length > MaximumRecordBytes)
                    throw new InvalidDataException("Navigation record exceeds its size limit.");
                var cost = checked(buffer.Length + sizeof(int) + sizeof(long));
                if (cost > maximumIndexBytes - records.Position -
                    headings.Position - links.Position)
                    throw new InvalidDataException("Markdown navigation index exceeds its disk budget.");

                var offset = records.Position;
                recordWriter.Write(checked((int)buffer.Length));
                recordWriter.Write(buffer.GetBuffer(), 0, checked((int)buffer.Length));
                if (item.Kind == MdViewIndexedItemKind.Heading)
                {
                    headingWriter.Write(offset);
                    headingCount++;
                }
                else
                {
                    linkWriter.Write(offset);
                    linkCount++;
                }
            }

            using var reader = snapshot.OpenReader();
            await foreach (var line in MarkdownIncrementalContainerScanner.ScanAsync(
                reader, maxLineCharacters: MarkdownIncrementalLineScanner.DefaultMaxLineCharacters,
                cancellationToken: cancellationToken)
                .WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                cancellationToken.ThrowIfCancellationRequested();
                scanned++;
                if (line.Kind is MarkdownContainerLineKind.FenceOpen or
                    MarkdownContainerLineKind.CodeLine or
                    MarkdownContainerLineKind.FenceClose)
                    continue;

                // Conservative: only unquoted top-level ATX headings.
                if (line.Kind == MarkdownContainerLineKind.Plain &&
                    line.Source.Kind == MarkdownStreamLineKind.Heading)
                {
                    var source = line.Source.Text.TrimStart(' ');
                    var title = source[line.Source.HeadingLevel..]
                        .Trim().TrimEnd('#').TrimEnd();
                    Store(new MdViewIndexedNavigationItem(
                        MdViewIndexedItemKind.Heading,
                        line.Source.LineNumber, line.Source.Utf16Offset,
                        Clip(title), null));
                }

                // Inline parser ignores code spans, escaped syntax and
                // images. Quoted/list text is allowed; full multiline
                // reference definitions are intentionally not indexed.
                foreach (var item in WalkLinks(
                    MarkdownInlineParser.Parse(line.Source.Text)))
                {
                    if (item.Destination is null ||
                        item.Destination.Length > MaximumDestinationCharacters)
                        continue;
                    Store(new MdViewIndexedNavigationItem(
                        MdViewIndexedItemKind.Link,
                        line.Source.LineNumber,
                        checked(line.Source.Utf16Offset + item.Start),
                        Clip(item.Text), item.Destination));
                }
            }

            if (scanned != snapshot.Lines.LineCount)
                throw new InvalidDataException(
                    "Scanner source-line count differs from the validated snapshot.");

            recordWriter.Flush();
            headingWriter.Flush();
            linkWriter.Flush();
            await records.FlushAsync(cancellationToken).ConfigureAwait(false);
            await headings.FlushAsync(cancellationToken).ConfigureAwait(false);
            await links.FlushAsync(cancellationToken).ConfigureAwait(false);
            return new MdViewDiskNavigationIndex(
                records, headings, links, headingCount, linkCount, scanned);
        }
        catch
        {
            if (links is not null)
                await links.DisposeAsync().ConfigureAwait(false);
            if (headings is not null)
                await headings.DisposeAsync().ConfigureAwait(false);
            await records.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }

    /// <summary>Reads one bounded ordered page of heading or link source coordinates.</summary>
    /// <param name="kind">Result category.</param>
    /// <param name="firstIndex">Zero-based ordinal among entries of that category.</param>
    /// <param name="maxItems">Maximum results, between 1 and 128.</param>
    /// <param name="cancellationToken">Cancels reading without changing the index.</param>
    /// <returns>Immutable snapshot-independent result values for the selected page.</returns>
    public async Task<IReadOnlyList<MdViewIndexedNavigationItem>> ReadAsync(
        MdViewIndexedItemKind kind, long firstIndex, int maxItems = 20,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (kind is not (MdViewIndexedItemKind.Heading or MdViewIndexedItemKind.Link))
            throw new ArgumentOutOfRangeException(nameof(kind));
        var total = kind == MdViewIndexedItemKind.Heading ? HeadingCount : LinkCount;
        if (firstIndex < 0 || firstIndex > total)
            throw new ArgumentOutOfRangeException(nameof(firstIndex));
        if (maxItems is < 1 or > MaximumPageItems)
            throw new ArgumentOutOfRangeException(nameof(maxItems));

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            var offsets = kind == MdViewIndexedItemKind.Heading ? _headings : _links;
            offsets.Position = checked(firstIndex * sizeof(long));
            var result = new List<MdViewIndexedNavigationItem>(
                checked((int)Math.Min(maxItems, total - firstIndex)));
            using var offsetReader = new BinaryReader(offsets, Encoding.UTF8, leaveOpen: true);
            using var recordReader = new BinaryReader(_records, Encoding.UTF8, leaveOpen: true);
            for (var i = 0; i < maxItems && firstIndex + i < total; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var recordOffset = offsetReader.ReadInt64();
                if (recordOffset < 0 || recordOffset > _records.Length - sizeof(int))
                    throw new InvalidDataException("Indexed navigation record offset is invalid.");
                _records.Position = recordOffset;
                var size = recordReader.ReadInt32();
                if (size is < 1 or > MaximumRecordBytes ||
                    size > _records.Length - recordOffset - sizeof(int))
                    throw new InvalidDataException("Indexed navigation record length is invalid.");
                var bytes = recordReader.ReadBytes(size);
                if (bytes.Length != size)
                    throw new EndOfStreamException("Indexed navigation record is incomplete.");
                using var payload = new BinaryReader(
                    new MemoryStream(bytes, writable: false), Encoding.UTF8);
                var storedKind = (MdViewIndexedItemKind)payload.ReadByte();
                var line = payload.ReadInt64();
                var offset = payload.ReadInt64();
                var label = payload.ReadString();
                var destination = payload.ReadString();
                if (storedKind != kind || line < 0 || line >= SourceLineCount ||
                    offset < 0 || label.Length > MaximumLabelCharacters ||
                    destination.Length > MaximumDestinationCharacters ||
                    payload.BaseStream.Position != payload.BaseStream.Length)
                    throw new InvalidDataException("Indexed Markdown source record is inconsistent.");
                result.Add(new MdViewIndexedNavigationItem(
                    kind, line, offset, label,
                    kind == MdViewIndexedItemKind.Link ? destination : null));
            }
            return result;
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>Deletes the temporary index files without disposing the caller's snapshot.</summary>
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
            await _links.DisposeAsync().ConfigureAwait(false);
            await _headings.DisposeAsync().ConfigureAwait(false);
            await _records.DisposeAsync().ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }

    private static IEnumerable<MarkdownInlineNode> WalkLinks(
        IReadOnlyList<MarkdownInlineNode> nodes)
    {
        foreach (var node in nodes)
        {
            if (node.Kind == MarkdownInlineKind.Link)
                yield return node;
            else if (node.Children.Count > 0)
                foreach (var child in WalkLinks(node.Children))
                    yield return child;
        }
    }

    private static string Clip(string text)
    {
        if (text.Length <= MaximumLabelCharacters)
            return text;
        var count = MaximumLabelCharacters;
        if (char.IsHighSurrogate(text[count - 1]))
            count--;
        return text[..count];
    }

    private static FileStream CreateTemp()
    {
        var path = Path.Combine(Path.GetTempPath(),
            "afrowave-mdview-navigation-" + Guid.NewGuid().ToString("N") + ".idx");
        return new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite,
            FileShare.None, 65536, FileOptions.Asynchronous | FileOptions.DeleteOnClose);
    }
}
