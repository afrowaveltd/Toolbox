using System.Text.Json;

namespace Afrowave.Toolbox.Blackboard.Showcase;

/// <summary>The result of verifying a small JSON file before terminal rendering.</summary>
public enum JsonPreviewKind
{
    /// <summary>The complete selected file contains valid JSON.</summary>
    Valid,
    /// <summary>A file cannot safely be displayed as text.</summary>
    BinaryOrUnsupportedEncoding,
    /// <summary>The JSON file exceeds the configured preview byte limit.</summary>
    TooLarge,
    /// <summary>The selected text is not valid JSON.</summary>
    InvalidJson
}

/// <summary>Bounded, read-only JSON preview prepared for Spectre.Console.Json.</summary>
/// <param name="Kind">Validation result.</param>
/// <param name="Json">Complete validated JSON text, or empty for other results.</param>
/// <param name="Details">Safe reason for non-valid JSON, without raw file contents.</param>
/// <param name="TotalBytes">File length at read time.</param>
public sealed record JsonPreviewResult(
    JsonPreviewKind Kind, string Json, string? Details, long TotalBytes);

/// <summary>
/// Loads only small, complete JSON documents and validates them with System.Text.Json.
/// </summary>
/// <remarks>
/// The showcase never asks Spectre's JSON renderer to parse a truncated, binary, or
/// malformed file. No preview code writes or modifies a selected file.
/// </remarks>
public static class JsonFilePreviewReader
{
    /// <summary>Maximum accepted JSON file size: 64 KiB.</summary>
    public const int MaxJsonPreviewBytes = 64 * 1024;

    /// <summary>Reads and validates an entire small JSON file for a colored preview.</summary>
    public static async Task<JsonPreviewResult> ReadAsync(
        string path, CancellationToken cancellationToken = default)
    {
        var text = await TextFilePreviewReader.ReadAsync(
            path, MaxJsonPreviewBytes, cancellationToken).ConfigureAwait(false);

        if (text.Truncated)
            return new JsonPreviewResult(
                JsonPreviewKind.TooLarge, "", "JSON is larger than the 64 KiB preview limit.",
                text.TotalBytes);

        if (text.Kind is TextPreviewKind.BinaryOrUnsupportedEncoding)
            return new JsonPreviewResult(
                JsonPreviewKind.BinaryOrUnsupportedEncoding, "",
                "Binary content, unsupported encoding or terminal control character.",
                text.TotalBytes);

        if (text.Kind is TextPreviewKind.Empty)
            return new JsonPreviewResult(
                JsonPreviewKind.InvalidJson, "", "The file is empty.", text.TotalBytes);

        try
        {
            // Parse before handing untrusted JSON to the syntax-highlighting renderer.
            using var document = JsonDocument.Parse(text.Content, new JsonDocumentOptions
            {
                AllowTrailingCommas = false,
                CommentHandling = JsonCommentHandling.Disallow,
                MaxDepth = 64
            });
            return new JsonPreviewResult(JsonPreviewKind.Valid, text.Content, null, text.TotalBytes);
        }
        catch (JsonException ex)
        {
            return new JsonPreviewResult(
                JsonPreviewKind.InvalidJson, "",
                $"Invalid JSON at line {ex.LineNumber}, byte position {ex.BytePositionInLine}.",
                text.TotalBytes);
        }
    }
}
