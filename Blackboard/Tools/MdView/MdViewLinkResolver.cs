namespace Afrowave.Toolbox.Blackboard.Tools.MdView;

/// <summary>Origin type used by the optional Markdown relative-link resolver.</summary>
public enum MdViewDocumentOriginKind
{
    /// <summary>A local Markdown file within a configured navigation root.</summary>
    Local,
    /// <summary>A public HTTPS Markdown resource.</summary>
    Https,
    /// <summary>A Markdown file within a specific GitHub repository and ref.</summary>
    GitHub
}

/// <summary>
/// Typed source identity for resolving relative Markdown links. A local
/// navigation root is a lexical boundary, not a symlink or filesystem ACL.
/// </summary>
/// <param name="Kind">Source type.</param>
/// <param name="Location">Absolute local filename, absolute HTTPS URL, or repository-relative file path.</param>
/// <param name="LocalRoot">Permitted local directory tree for local navigation.</param>
/// <param name="Owner">GitHub repository owner.</param>
/// <param name="Repository">GitHub repository name.</param>
/// <param name="Ref">Explicit GitHub branch, tag or commit identifier.</param>
public sealed record MdViewDocumentOrigin(
    MdViewDocumentOriginKind Kind, string Location,
    string? LocalRoot = null, string? Owner = null,
    string? Repository = null, string? Ref = null)
{
    /// <summary>Creates a local source with a caller-selected navigation root.</summary>
    /// <param name="file">Current local document filename.</param>
    /// <param name="root">Directory allowed for relative navigation.</param>
    /// <returns>Canonical local origin.</returns>
    public static MdViewDocumentOrigin FromLocal(string file, string root)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(file);
        ArgumentException.ThrowIfNullOrWhiteSpace(root);
        var directory = Path.GetFullPath(root);
        var path = Path.GetFullPath(file);
        if (!MdViewLinkResolver.IsWithinRoot(path, directory))
            throw new ArgumentException("Document lies outside the navigation root.", nameof(file));
        return new(MdViewDocumentOriginKind.Local, path, directory);
    }

    /// <summary>Creates an HTTPS document origin without a fragment.</summary>
    /// <param name="url">Absolute HTTPS document URI.</param>
    /// <returns>Canonical HTTPS origin.</returns>
    public static MdViewDocumentOrigin FromHttps(string url)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            uri.Scheme != Uri.UriSchemeHttps || uri.UserInfo.Length > 0 ||
            uri.Query.Length > 0 || uri.Fragment.Length > 0)
            throw new ArgumentException("Expected an HTTPS document URL without credentials, query or fragment.", nameof(url));
        return new(MdViewDocumentOriginKind.Https, uri.AbsoluteUri);
    }

    /// <summary>Creates a file origin within a specified GitHub repository and resolved ref.</summary>
    /// <param name="owner">Repository owner.</param>
    /// <param name="repository">Repository name.</param>
    /// <param name="reference">Resolved ref, not a guessed default branch.</param>
    /// <param name="path">Repository-relative Markdown filename.</param>
    /// <returns>Canonical GitHub file origin.</returns>
    public static MdViewDocumentOrigin FromGitHub(
        string owner, string repository, string reference, string path)
    {
        if (!MdViewLinkResolver.ValidGithubName(owner) ||
            !MdViewLinkResolver.ValidGithubName(repository) ||
            string.IsNullOrWhiteSpace(reference) ||
            reference.Any(c => char.IsControl(c) || c == '\\' || c == '?' || c == '#') ||
            !MdViewLinkResolver.TryNormalizeRepositoryPath(path, out var normalized) ||
            normalized.Length == 0)
            throw new ArgumentException("Invalid GitHub source identity.");
        return new(MdViewDocumentOriginKind.GitHub, normalized,
            Owner: owner, Repository: repository, Ref: reference);
    }
}

/// <summary>A resolved same-source Markdown link; no I/O or navigation is performed.</summary>
/// <param name="Origin">Target source, preserving the original source type.</param>
/// <param name="Fragment">Optional decoded fragment without its leading hash.</param>
public sealed record MdViewResolvedLink(MdViewDocumentOrigin Origin, string? Fragment);

/// <summary>
/// Pure resolver for relative Markdown documents and anchors. It never opens,
/// downloads or executes the target. Cross-origin and unsafe schemes are rejected.
/// </summary>
public static class MdViewLinkResolver
{
    /// <summary>Resolves a Markdown link relative to the referring document.</summary>
    /// <param name="source">Typed current document origin.</param>
    /// <param name="href">Relative Markdown target or same-document fragment.</param>
    /// <returns>Canonical target origin and optional anchor.</returns>
    /// <exception cref="ArgumentException">A target is unsupported or leaves the permitted scope.</exception>
    public static MdViewResolvedLink Resolve(MdViewDocumentOrigin source, string href)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentException.ThrowIfNullOrWhiteSpace(href);
        if (href.Any(char.IsControl) || href.Contains('\\') ||
            href.Contains('?') || href.Contains('%') ||
            href.StartsWith("//", StringComparison.Ordinal))
            throw new ArgumentException("Unsupported or encoded Markdown link.", nameof(href));

        var hash = href.IndexOf('#');
        var linkPath = hash < 0 ? href : href[..hash];
        var fragment = hash < 0 ? null : Uri.UnescapeDataString(href[(hash + 1)..]);
        if (fragment is not null && (fragment.Any(char.IsControl) ||
            fragment.Contains('%') || fragment.Contains('/')))
            throw new ArgumentException("Invalid Markdown fragment.", nameof(href));
        if (linkPath.Contains(':'))
            throw new ArgumentException("External and scheme-qualified links require explicit handling.", nameof(href));

        if (linkPath.Length == 0)
            return new(source, fragment);

        switch (source.Kind)
        {
            case MdViewDocumentOriginKind.Local:
            {
                if (source.LocalRoot is null ||
                    Path.IsPathRooted(linkPath))
                    throw new ArgumentException("Invalid local document base or absolute link.", nameof(source));
                var destination = Path.GetFullPath(Path.Combine(
                    Path.GetDirectoryName(source.Location)!, linkPath.Replace('/', Path.DirectorySeparatorChar)));
                if (!IsWithinRoot(destination, source.LocalRoot))
                    throw new ArgumentException("Link escapes the local navigation root.", nameof(href));
                return new(MdViewDocumentOrigin.FromLocal(destination, source.LocalRoot), fragment);
            }
            case MdViewDocumentOriginKind.Https:
            {
                var baseUri = new Uri(source.Location, UriKind.Absolute);
                if (baseUri.Scheme != Uri.UriSchemeHttps || baseUri.UserInfo.Length > 0 ||
                    Path.IsPathRooted(linkPath) && !linkPath.StartsWith('/'))
                    throw new ArgumentException("Invalid HTTPS source base.", nameof(source));
                var target = new Uri(baseUri, linkPath);
                if (target.Scheme != Uri.UriSchemeHttps ||
                    target.Host != baseUri.Host || target.Port != baseUri.Port ||
                    target.UserInfo.Length > 0 || target.Query.Length > 0)
                    throw new ArgumentException("Link leaves the HTTPS source origin.", nameof(href));
                return new(MdViewDocumentOrigin.FromHttps(target.AbsoluteUri), fragment);
            }
            case MdViewDocumentOriginKind.GitHub:
            {
                if (source.Owner is null || source.Repository is null || source.Ref is null ||
                    linkPath.StartsWith('/'))
                    throw new ArgumentException("Invalid GitHub source base or absolute link.", nameof(source));
                var slash = source.Location.LastIndexOf('/');
                var prefix = slash < 0 ? "" : source.Location[..(slash + 1)];
                if (!TryNormalizeRepositoryPath(prefix + linkPath, out var normalized) ||
                    normalized.Length == 0)
                    throw new ArgumentException("Link escapes the GitHub repository.", nameof(href));
                return new(MdViewDocumentOrigin.FromGitHub(
                    source.Owner, source.Repository, source.Ref, normalized), fragment);
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(source));
        }
    }

    internal static bool IsWithinRoot(string path, string root)
    {
        var normalizedRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(root));
        var normalizedPath = Path.GetFullPath(path);
        var comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        return normalizedPath.Equals(normalizedRoot, comparison) ||
            normalizedPath.StartsWith(normalizedRoot + Path.DirectorySeparatorChar, comparison);
    }

    internal static bool ValidGithubName(string? name) =>
        !string.IsNullOrWhiteSpace(name) && name.Length <= 100 &&
        name.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_' or '.');

    internal static bool TryNormalizeRepositoryPath(string? path, out string normalized)
    {
        normalized = "";
        if (string.IsNullOrWhiteSpace(path) || path.StartsWith('/') ||
            path.Contains('\\') || path.Contains('%') || path.Contains('?') ||
            path.Contains('#') || path.Any(char.IsControl))
            return false;
        var segments = new List<string>();
        foreach (var part in path.Split('/'))
        {
            if (part is "" or ".") continue;
            if (part == "..")
            {
                if (segments.Count == 0) return false;
                segments.RemoveAt(segments.Count - 1);
            }
            else if (part.Contains(':'))
                return false;
            else
                segments.Add(part);
        }
        normalized = string.Join("/", segments);
        return true;
    }
}
