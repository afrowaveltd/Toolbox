namespace Afrowave.Toolbox.Blackboard.Tools.MdView;

/// <summary>Supported modes and source types for the mdview command-line reader.</summary>
public enum MdViewSourceKind
{
    /// <summary>Open the interactive Spectre.Console source selection menu.</summary>
    Interactive,
    /// <summary>Read a local Markdown document with full rendering.</summary>
    Local,
    /// <summary>Read a public HTTP(S) Markdown document.</summary>
    Url,
    /// <summary>Read a GitHub README or browse an owner's public repositories.</summary>
    GitHub,
    /// <summary>Print command-line usage and exit.</summary>
    Help,
    /// <summary>Navigate a local UTF-8 file as indexed raw source lines.</summary>
    SourcePager,
    /// <summary>Preview one bounded source page with the Markdown renderer.</summary>
    FormattedPreview,
    /// <summary>Browse local Markdown links with explicit keyboard navigation.</summary>
    LocalLinkBrowser,
    /// <summary>Browse same-origin HTTPS Markdown links with verified caching.</summary>
    HttpsLinkBrowser
}

/// <summary>Parsed mdview invocation, independent of terminal interaction and network I/O.</summary>
/// <param name="Kind">Selected reader mode or command action.</param>
/// <param name="Value">Local path, remote URL or GitHub owner, depending on the mode.</param>
/// <param name="Repository">Optional GitHub repository; null requests owner-only browsing.</param>
public sealed record MdViewCommand(MdViewSourceKind Kind, string? Value = null, string? Repository = null)
{
    /// <summary>Converts command-line tokens to a strongly typed source command.</summary>
    /// <param name="args">CLI arguments, excluding the executable name.</param>
    /// <returns>A command describing the selected source or interactive/help action.</returns>
    /// <exception cref="ArgumentNullException">The argument array is null.</exception>
    /// <exception cref="ArgumentException">The source or command syntax is invalid.</exception>
    public static MdViewCommand Parse(params string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);
        if (args.Length == 0) return new(MdViewSourceKind.Interactive);
        if (args.Length == 1)
        {
            var value = args[0];
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Empty source.");
            if (value is "-h" or "--help" or "/?") return new(MdViewSourceKind.Help);
            if (value.StartsWith("github:", StringComparison.OrdinalIgnoreCase))
            {
                var parts = value["github:".Length..].Split('/');
                if (parts.Length == 1 && IsOwner(parts[0]))
                    return new(MdViewSourceKind.GitHub, parts[0]);
                if (parts.Length != 2 || !IsOwner(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
                    throw new ArgumentException("Use github:OWNER or github:OWNER/REPOSITORY.");
                return new(MdViewSourceKind.GitHub, parts[0], parts[1]);
            }
            if (Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
                uri.Scheme is "http" or "https")
                return new(MdViewSourceKind.Url, value);
            // No extension/separator and a valid GitHub owner: discover public repos.
            // Existing README.md and explicit local paths retain their local meaning.
            if (IsOwner(value))
                return new(MdViewSourceKind.GitHub, value);
            return new(MdViewSourceKind.Local, value);
        }
        if (args.Length == 2)
        {
            if (args[0].Equals("local", StringComparison.OrdinalIgnoreCase))
                return new(MdViewSourceKind.Local, Require(args[1]));
            if (args[0].Equals("url", StringComparison.OrdinalIgnoreCase))
                return new(MdViewSourceKind.Url, Require(args[1]));
            if (args[0].Equals("source", StringComparison.OrdinalIgnoreCase))
                return new(MdViewSourceKind.SourcePager, Require(args[1]));
            if (args[0].Equals("browse", StringComparison.OrdinalIgnoreCase))
                return new(MdViewSourceKind.LocalLinkBrowser, Require(args[1]));
            if (args[0].Equals("browse-url", StringComparison.OrdinalIgnoreCase))
                return new(MdViewSourceKind.HttpsLinkBrowser, Require(args[1]));
            if (args[0].Equals("preview", StringComparison.OrdinalIgnoreCase))
                return new(MdViewSourceKind.FormattedPreview, Require(args[1]));
            if (args[0].Equals("git", StringComparison.OrdinalIgnoreCase) ||
                args[0].Equals("github", StringComparison.OrdinalIgnoreCase))
            {
                var pieces = Require(args[1]).Split('/');
                if (pieces.Length == 1 && IsOwner(pieces[0]))
                    return new(MdViewSourceKind.GitHub, pieces[0]);
                if (pieces.Length != 2 || !IsOwner(pieces[0]) || string.IsNullOrWhiteSpace(pieces[1]))
                    throw new ArgumentException("Use git OWNER, git OWNER REPOSITORY, or git OWNER/REPOSITORY.");
                return new(MdViewSourceKind.GitHub, pieces[0], pieces[1]);
            }
        }
        if (args.Length == 3 &&
            (args[0].Equals("git", StringComparison.OrdinalIgnoreCase) ||
             args[0].Equals("github", StringComparison.OrdinalIgnoreCase)))
            return new(MdViewSourceKind.GitHub, Require(args[1]), Require(args[2]));
        throw new ArgumentException("Unsupported arguments. Use --help for command syntax.");
    }

    // GitHub login names are ASCII alphanumeric with optional hyphens.
    // Keeping this local to command parsing avoids an I/O-dependent guess for paths.
    private static bool IsOwner(string value) =>
        value.Length is >= 1 and <= 39 &&
        char.IsAsciiLetterOrDigit(value[0]) &&
        value.All(character => char.IsAsciiLetterOrDigit(character) || character == '-');

    private static string Require(string value) =>
        !string.IsNullOrWhiteSpace(value) ? value : throw new ArgumentException("Source cannot be empty.");
}
