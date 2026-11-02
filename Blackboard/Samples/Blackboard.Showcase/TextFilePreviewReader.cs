using System.Text;

namespace Afrowave.Toolbox.Blackboard.Showcase;

/// <summary>How a selected local file can be represented safely in a terminal.</summary>
public enum TextPreviewKind
{
    /// <summary>A valid supported text encoding with printable content.</summary>
    Text,
    /// <summary>An empty file or file containing only a byte-order mark.</summary>
    Empty,
    /// <summary>Binary, unsafe control codes or an unsupported legacy encoding.</summary>
    BinaryOrUnsupportedEncoding
}

/// <summary>Bounded read-only text preview, never the entire arbitrary file.</summary>
/// <param name="Kind">Detected presentation category.</param>
/// <param name="Content">Preview text, empty unless Kind is Text.</param>
/// <param name="EncodingName">Detected encoding, or null when unsupported.</param>
/// <param name="Truncated">True when more bytes exist beyond the bounded read.</param>
/// <param name="TotalBytes">Original file size at the time of the read.</param>
public sealed record TextPreviewResult(
    TextPreviewKind Kind, string Content, string? EncodingName, bool Truncated, long TotalBytes);

/// <summary>
/// Text-only, bounded, read-only local file preview used by Blackboard's example app.
/// </summary>
/// <remarks>
/// The classifier is intentionally conservative: unsupported code pages and binary
/// data are not decoded with replacement characters and never written to the terminal.
/// This is a demonstration, not a general-purpose file format/charset detector.
/// </remarks>
public static class TextFilePreviewReader
{
    /// <summary>Default maximum bytes printed from one selected file (16 KiB).</summary>
    public const int DefaultMaxPreviewBytes = 16 * 1024;

    /// <summary>
    /// Reads no more than the requested number of bytes from an existing local file.
    /// Supports strict UTF-8, UTF-8 BOM, UTF-16 BOM, and UTF-32 BOM.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The preview limit is out of bounds.</exception>
    public static async Task<TextPreviewResult> ReadAsync(
        string path, int maxPreviewBytes = DefaultMaxPreviewBytes,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (maxPreviewBytes is < 256 or > 256 * 1024)
            throw new ArgumentOutOfRangeException(nameof(maxPreviewBytes),
                "Preview size must be from 256 bytes to 256 KiB.");

        await using var stream = new FileStream(
            path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 4096,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        var originalLength = stream.Length;
        if (originalLength == 0)
            return new TextPreviewResult(TextPreviewKind.Empty, "", "UTF-8", false, 0);

        var bytes = new byte[maxPreviewBytes];
        var length = 0;
        while (length < bytes.Length)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var read = await stream.ReadAsync(bytes.AsMemory(length), cancellationToken)
                .ConfigureAwait(false);
            if (read == 0)
                break;
            length += read;
        }

        var truncated = originalLength > length;
        var (encoding, offset, encodingName) = DetectEncoding(bytes.AsSpan(0, length));
        var decodedLength = length - offset;

        // An undecorated UTF-8 text file must not contain NUL bytes. NULs are
        // normal in UTF-16/32, which are only accepted when a BOM was detected.
        if (offset == 0 && bytes.AsSpan(0, length).Contains((byte)0))
            return new TextPreviewResult(
                TextPreviewKind.BinaryOrUnsupportedEncoding, "", null, truncated, originalLength);

        // The preview limit may cut through a Unicode code point. Discard at most
        // one terminal partial code point, but never silently replace invalid bytes.
        var maxTrim = truncated ? Math.Min(decodedLength, 4) : 0;
        for (var trim = 0; trim <= maxTrim; trim++)
        {
            try
            {
                var text = encoding.GetString(bytes, offset, decodedLength - trim);
                if (HasUnsafeControls(text))
                    return new TextPreviewResult(
                        TextPreviewKind.BinaryOrUnsupportedEncoding, "", null, truncated, originalLength);

                return new TextPreviewResult(
                    text.Length == 0 ? TextPreviewKind.Empty : TextPreviewKind.Text,
                    text, encodingName, truncated, originalLength);
            }
            catch (DecoderFallbackException)
            {
                // A valid truncated UTF-8/16/32 tail may need a short trim.
            }
        }

        return new TextPreviewResult(
            TextPreviewKind.BinaryOrUnsupportedEncoding, "", null, truncated, originalLength);
    }

    /// <summary>
    /// Reads and validates an entire selected text file, without an arbitrary byte cap.
    /// Uses a small reusable decoding buffer; the returned document is materialized
    /// as one string for whole-document Markdown parsing and rendering.
    /// </summary>
    /// <remarks>
    /// Intended for user-initiated full documents, not the bounded generic file
    /// preview. Large documents consume memory proportional to decoded text and
    /// their Markdown syntax tree. Invalid encoding or terminal controls anywhere
    /// in the file reject the entire result.
    /// </remarks>
    public static async Task<TextPreviewResult> ReadCompleteAsync(
        string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        await using var stream = new FileStream(
            path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 8192,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        var originalLength = stream.Length;
        if (originalLength == 0)
            return new TextPreviewResult(TextPreviewKind.Empty, "", "UTF-8", false, 0);

        // Read enough initial bytes to distinguish UTF-8/16/32 BOMs even if
        // a file stream returns fewer bytes than requested in one read.
        var prefix = new byte[4];
        var prefixLength = 0;
        while (prefixLength < prefix.Length)
        {
            var read = await stream.ReadAsync(
                prefix.AsMemory(prefixLength), cancellationToken).ConfigureAwait(false);
            if (read == 0)
                break;
            prefixLength += read;
        }

        var (encoding, bomLength, encodingName) =
            DetectEncoding(prefix.AsSpan(0, prefixLength));
        stream.Position = bomLength;

        // StreamReader keeps decoder state across chunk boundaries, so a valid
        // multibyte code point split across reads is never mistaken for corruption.
        using var reader = new StreamReader(stream, encoding,
            detectEncodingFromByteOrderMarks: false, bufferSize: 8192, leaveOpen: true);
        var characters = new char[8192];
        var text = new StringBuilder();
        try
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var read = await reader.ReadAsync(
                    characters.AsMemory(), cancellationToken).ConfigureAwait(false);
                if (read == 0)
                    break;

                var chunk = characters.AsSpan(0, read);
                if (HasUnsafeControls(chunk))
                    return new TextPreviewResult(
                        TextPreviewKind.BinaryOrUnsupportedEncoding, "", null, false,
                        originalLength);
                text.Append(chunk);
            }
        }
        catch (DecoderFallbackException)
        {
            return new TextPreviewResult(
                TextPreviewKind.BinaryOrUnsupportedEncoding, "", null, false,
                originalLength);
        }

        return new TextPreviewResult(
            text.Length == 0 ? TextPreviewKind.Empty : TextPreviewKind.Text,
            text.ToString(), encodingName, false, originalLength);
    }

    private static (Encoding Encoding, int Offset, string Name) DetectEncoding(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length >= 4 && bytes[..4].SequenceEqual(new byte[] { 0xFF, 0xFE, 0x00, 0x00 }))
            return (new UTF32Encoding(false, true, true), 4, "UTF-32 LE");

        if (bytes.Length >= 4 && bytes[..4].SequenceEqual(new byte[] { 0x00, 0x00, 0xFE, 0xFF }))
            return (new UTF32Encoding(true, true, true), 4, "UTF-32 BE");

        if (bytes.Length >= 3 && bytes[..3].SequenceEqual(new byte[] { 0xEF, 0xBB, 0xBF }))
            return (new UTF8Encoding(true, true), 3, "UTF-8 BOM");

        if (bytes.Length >= 2 && bytes[..2].SequenceEqual(new byte[] { 0xFF, 0xFE }))
            return (new UnicodeEncoding(false, true, true), 2, "UTF-16 LE");

        if (bytes.Length >= 2 && bytes[..2].SequenceEqual(new byte[] { 0xFE, 0xFF }))
            return (new UnicodeEncoding(true, true, true), 2, "UTF-16 BE");

        return (new UTF8Encoding(false, true), 0, "UTF-8");
    }

    private static bool HasUnsafeControls(string text) =>
        HasUnsafeControls(text.AsSpan());

    private static bool HasUnsafeControls(ReadOnlySpan<char> text)
    {
        foreach (var character in text)
        {
            // Escape sequences could modify a terminal even when sent through a
            // plain Text renderable. Refuse the preview rather than printing them.
            if (char.IsControl(character) && character is not '\n' and not '\r' and not '\t')
                return true;
        }

        return false;
    }
}
