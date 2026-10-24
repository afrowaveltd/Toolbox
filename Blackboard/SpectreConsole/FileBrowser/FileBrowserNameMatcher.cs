using System.IO.Enumeration;
using System.Text.RegularExpressions;

namespace Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser;

/// <summary>How custom filename criteria are interpreted.</summary>
public enum FileBrowserPatternMode
{
    /// <summary>One or more semicolon-separated filename masks using '*' and '?'.</summary>
    Wildcard,
    /// <summary>A .NET regular expression applied to a single filename.</summary>
    RegularExpression
}

/// <summary>
/// Reusable filename matcher for directory filters and recursive search.
/// </summary>
/// <remarks>
/// Wildcard masks are OR-combined, case rules are configurable, and a regular
/// expression uses a bounded evaluation timeout. Nothing inspects file contents.
/// </remarks>
public sealed class FileBrowserNameMatcher
{
    private static readonly TimeSpan DefaultRegexTimeout = TimeSpan.FromMilliseconds(200);
    private readonly string[]? _masks;
    private readonly Regex? _regex;
    private readonly bool _ignoreCase;

    /// <summary>Creates an immutable filename matcher from a mask list or a regular expression.</summary>
    /// <param name="pattern">Example "*.ajis;*.json", "*", or regex "^[^.]+\\.(ajis|json)$".</param>
    /// <param name="mode">Wildcard by default, optional regular expression.</param>
    /// <param name="ignoreCase">Case-insensitive by default; sources may override.</param>
    /// <param name="regexTimeout">Optional bounded regex timeout (1 ms through 5 seconds).</param>
    public FileBrowserNameMatcher(
        string pattern, FileBrowserPatternMode mode = FileBrowserPatternMode.Wildcard,
        bool ignoreCase = true, TimeSpan? regexTimeout = null)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        if (!Enum.IsDefined(mode))
            throw new ArgumentOutOfRangeException(nameof(mode));
        if (string.IsNullOrWhiteSpace(pattern) || pattern.Length > 2048 ||
            pattern.IndexOfAny(['\0', '\r', '\n']) >= 0)
            throw new ArgumentException("Enter a non-empty filename mask or expression (maximum 2048 characters).",
                nameof(pattern));

        Mode = mode;
        Pattern = pattern;
        _ignoreCase = ignoreCase;

        if (mode == FileBrowserPatternMode.Wildcard)
        {
            if (pattern.Contains('/') || pattern.Contains('\\'))
                throw new ArgumentException("Masks must match file names, not paths.", nameof(pattern));

            _masks = pattern.Split(';', StringSplitOptions.TrimEntries);
            if (_masks.Length == 0 || _masks.Length > 64 ||
                _masks.Any(mask => string.IsNullOrWhiteSpace(mask)))
                throw new ArgumentException("Provide 1 to 64 non-empty masks separated by ';'.", nameof(pattern));
        }
        else
        {
            var timeout = regexTimeout ?? DefaultRegexTimeout;
            if (timeout < TimeSpan.FromMilliseconds(1) || timeout > TimeSpan.FromSeconds(5))
                throw new ArgumentOutOfRangeException(nameof(regexTimeout),
                    "Regex timeout must be between 1 millisecond and 5 seconds.");
            var options = RegexOptions.CultureInvariant |
                          (ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None);
            _regex = new Regex(pattern, options, timeout);
        }
    }

    /// <summary>The original user-entered filter or search expression.</summary>
    public string Pattern { get; }

    /// <summary>Filename matching mode.</summary>
    public FileBrowserPatternMode Mode { get; }

    /// <summary>Matches one filename; callers handle directory traversal separately.</summary>
    public bool Matches(string? filename)
    {
        if (string.IsNullOrEmpty(filename))
            return false;

        if (_regex is not null)
            return _regex.IsMatch(filename);

        return _masks!.Any(mask =>
            FileSystemName.MatchesSimpleExpression(mask, filename, _ignoreCase));
    }
}
