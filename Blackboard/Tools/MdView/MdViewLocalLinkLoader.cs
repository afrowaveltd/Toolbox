using Afrowave.Toolbox.Blackboard.TextFiles;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView;

/// <summary>
/// A local linked Markdown document backed by an independently owned,
/// validated temporary UTF-8 snapshot. Dispose it after reading.
/// </summary>
/// <param name="Target">Resolved document origin and optional fragment.</param>
/// <param name="Snapshot">Fully validated copy; owned by this result.</param>
public sealed record MdViewLocalLinkedDocument(
    MdViewResolvedLink Target, Utf8ValidatedSnapshot Snapshot) : IAsyncDisposable
{
    /// <summary>Releases the validated copy and its temporary index.</summary>
    public ValueTask DisposeAsync() => Snapshot.DisposeAsync();
}

/// <summary>
/// Opens only explicitly requested local Markdown links inside a lexical root,
/// rejecting symlink/reparse-point paths before copying into a validated snapshot.
/// </summary>
/// <remarks>
/// Path checks are best-effort checks before opening, not race-free or a
/// malicious same-user filesystem sandbox. A concurrent directory substitution
/// can occur between checking and opening. Do not treat this as authorization
/// to follow arbitrary untrusted file trees without stronger OS-level controls.
/// Online/GitHub links require separate network and cache loaders.
/// </remarks>
public static class MdViewLocalLinkLoader
{
    /// <summary>
    /// Resolves and opens a local Markdown link into a bounded, fully validated
    /// private snapshot without loading the entire document into RAM.
    /// </summary>
    /// <param name="source">Current local document and its permitted navigation root.</param>
    /// <param name="href">Relative local Markdown path, optionally with a fragment.</param>
    /// <param name="maximumSnapshotBytes">Maximum source bytes staged on disk.</param>
    /// <param name="cancellationToken">Cancels opening and validation.</param>
    /// <returns>Caller-owned local linked document and its snapshot.</returns>
    public static async Task<MdViewLocalLinkedDocument> OpenAsync(
        MdViewDocumentOrigin source, string href,
        long maximumSnapshotBytes = Utf8ValidatedSnapshot.DefaultMaximumSnapshotBytes,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (source.Kind != MdViewDocumentOriginKind.Local ||
            string.IsNullOrWhiteSpace(source.LocalRoot))
            throw new ArgumentException("Local Markdown origin required.", nameof(source));
        cancellationToken.ThrowIfCancellationRequested();
        var target = MdViewLinkResolver.Resolve(source, href);
        if (target.Origin.Kind != MdViewDocumentOriginKind.Local)
            throw new ArgumentException("Only local linked Markdown files are supported.", nameof(href));

        var path = target.Origin.Location;
        var extension = Path.GetExtension(path);
        if (!extension.Equals(".md", StringComparison.OrdinalIgnoreCase) &&
            !extension.Equals(".markdown", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Link target is not a Markdown document.", nameof(href));

        CheckNoReparsePoints(path, target.Origin.LocalRoot!);
        var snapshot = await Utf8ValidatedSnapshot.CreateAsync(
            path, maximumSnapshotBytes: maximumSnapshotBytes,
            cancellationToken: cancellationToken).ConfigureAwait(false);
        return new MdViewLocalLinkedDocument(target, snapshot);
    }

    private static void CheckNoReparsePoints(string path, string root)
    {
        var fullRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(root));
        var fullPath = Path.GetFullPath(path);
        if (!MdViewLinkResolver.IsWithinRoot(fullPath, fullRoot))
            throw new ArgumentException("Link target escapes the navigation root.", nameof(path));

        // Inspect the root and every child component; checking the leaf only
        // would miss an intermediate symlink that changes the destination.
        RejectReparse(fullRoot);
        var relative = Path.GetRelativePath(fullRoot, fullPath);
        var current = fullRoot;
        foreach (var part in relative.Split(Path.DirectorySeparatorChar))
        {
            if (part is "." or "") continue;
            current = Path.Combine(current, part);
            RejectReparse(current);
        }
    }

    private static void RejectReparse(string path)
    {
        if ((File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)
            throw new IOException("Local Markdown link passes through a symbolic link or reparse point.");
    }
}
