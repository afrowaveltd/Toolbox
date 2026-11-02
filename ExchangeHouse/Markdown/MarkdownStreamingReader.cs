using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;

namespace Afrowave.Toolbox.ExchangeHouse.Markdown;

/// <summary>
/// Memory limits for the opt-in incremental Markdown chunk scanner.
/// A chunk is a self-contained parsing window, not a complete CommonMark document.
/// </summary>
public sealed class MarkdownStreamOptions
{
    public int PreferredChunkChars { get; init; } = 32 * 1024;
    public int MaxChunkChars { get; init; } = 256 * 1024;
    public int MaxLineChars { get; init; } = 64 * 1024;
    public int ReadBufferChars { get; init; } = 8 * 1024;

    internal void Validate()
    {
        if (PreferredChunkChars < 1 || MaxChunkChars < PreferredChunkChars ||
            MaxLineChars < 1 || MaxLineChars > MaxChunkChars ||
            ReadBufferChars is < 1 or > 1024 * 1024)
            throw new ArgumentOutOfRangeException(nameof(MarkdownStreamOptions),
                "Require 1 <= preferred <= maximum, 1 <= max line <= maximum chunk, and a positive bounded read buffer.");
    }
}

/// <summary>
/// Bounded, forward-only Markdown parsing windows from any TextReader, including
/// virtual readers that generate data without storing it on disk.
/// </summary>
/// <remarks>
/// Only blank-line boundaries outside simple fenced code are emitted. Each resulting
/// MarkdownDocument has chunk-local offsets and its own list/quote hierarchy.
/// Cross-window list ownership, references and complete CommonMark equivalence
/// are NOT guaranteed. Oversized unbroken blocks fail explicitly instead of
/// silently allocating unbounded memory. This API is experimental; do not
/// substitute it for a fully stateful incremental parser where exact semantics
/// or global offsets are required.
/// </remarks>
public static class MarkdownStreamingReader
{
    public static async IAsyncEnumerable<MarkdownDocument> ParseAsync(
        TextReader reader,
        MarkdownStreamOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(reader);
        options ??= new MarkdownStreamOptions();
        options.Validate();

        var chunk = new StringBuilder(Math.Min(options.PreferredChunkChars, 32 * 1024));
        char openFence = '\0';
        var fenceLength = 0;

        await foreach (var raw in ReadLinesAsync(reader, options, cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var line = raw.TrimEnd('\r', '\n');

            if (TryFence(line, out var marker, out var length, out var isCloser))
            {
                if (openFence == '\0')
                {
                    openFence = marker;
                    fenceLength = length;
                }
                else if (openFence == marker && isCloser && length >= fenceLength)
                {
                    openFence = '\0';
                    fenceLength = 0;
                }
            }

            if (chunk.Length > options.MaxChunkChars - raw.Length)
                throw new InvalidDataException(
                    "Markdown logical block exceeds MaxChunkChars. Increase the explicit " +
                    "limit or use a finer-grained stateful parser; input was not truncated.");

            chunk.Append(raw);

            if (openFence == '\0' && string.IsNullOrWhiteSpace(line) &&
                chunk.Length >= options.PreferredChunkChars)
            {
                yield return MarkdownParser.Parse(chunk.ToString());
                chunk.Clear();
            }
        }

        if (chunk.Length != 0)
            yield return MarkdownParser.Parse(chunk.ToString());
    }

    private static bool TryFence(string line, out char marker, out int length, out bool isCloser)
    {
        marker = '\0';
        length = 0;
        isCloser = false;

        var span = line.AsSpan();
        // Allow conservative quote-prefix fences; the full parser remains
        // responsible for actual syntax interpretation.
        while (!span.IsEmpty)
        {
            var spaces = 0;
            while (spaces < span.Length && spaces < 4 && span[spaces] == ' ')
                spaces++;
            if (spaces > 3)
                return false;
            span = span[spaces..];
            if (span.IsEmpty || span[0] != '>')
                break;
            span = span[1..];
            if (!span.IsEmpty && span[0] == ' ')
                span = span[1..];
        }

        var indent = 0;
        while (indent < span.Length && indent < 4 && span[indent] == ' ')
            indent++;
        if (indent > 3)
            return false;
        span = span[indent..];

        if (span.Length < 3 || (span[0] != '~' && span[0] != '\u0060'))
            return false;
        marker = span[0];
        while (length < span.Length && span[length] == marker)
            length++;
        if (length < 3)
            return false;
        isCloser = span[length..].Trim().IsEmpty;
        return true;
    }

    private static async IAsyncEnumerable<string> ReadLinesAsync(
        TextReader reader, MarkdownStreamOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var buffer = ArrayPool<char>.Shared.Rent(options.ReadBufferChars);
        var line = new StringBuilder();
        var afterCr = false;
        try
        {
            int read;
            while ((read = await reader.ReadAsync(
                       buffer.AsMemory(0, options.ReadBufferChars), cancellationToken)) != 0)
            {
                for (var i = 0; i < read; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var c = buffer[i];
                    if (afterCr)
                    {
                        if (c == '\n')
                        {
                            if (line.Length >= options.MaxLineChars)
                                throw new InvalidDataException("Markdown line exceeds MaxLineChars.");
                            line.Append('\n');
                            yield return line.ToString();
                            line.Clear();
                            afterCr = false;
                            continue;
                        }
                        yield return line.ToString();
                        line.Clear();
                        afterCr = false;
                    }

                    if (line.Length >= options.MaxLineChars)
                        throw new InvalidDataException("Markdown line exceeds MaxLineChars.");
                    line.Append(c);
                    if (c == '\n')
                    {
                        yield return line.ToString();
                        line.Clear();
                    }
                    else if (c == '\r')
                    {
                        afterCr = true;
                    }
                }
            }

            if (line.Length > 0)
                yield return line.ToString();
        }
        finally
        {
            ArrayPool<char>.Shared.Return(buffer);
        }
    }
}
