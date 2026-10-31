using System.Globalization;
using Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown;
using Spectre.Console;
using TextMateSharp.Grammars;
using TextMateSharp.Themes;

namespace Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown.TextMate;

/// <summary>Optional, source-only TextMate grammar highlighting backend.</summary>
public sealed class MarkdownTextMateHighlighter : IMarkdownSyntaxHighlighter
{
    private readonly RegistryOptions options;
    private readonly TextMateSharp.Registry.Registry registry;
    private readonly object sync = new();

    /// <summary>Load bundled grammars with a caller-selected TextMate color theme.</summary>
    public MarkdownTextMateHighlighter(ThemeName theme = ThemeName.DarkPlus)
    {
        options = new RegistryOptions(theme);
        registry = new TextMateSharp.Registry.Registry(options);
    }

    /// <inheritdoc/>
    public IMarkdownHighlightSession? CreateSession(string language)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(language);
        var id = NormalizeLanguage(language);
        if (id is "text" or "plain" or "none")
            return null;
        var scope = options.GetScopeByLanguageId(id);
        if (scope is null)
            return null;

        lock (sync)
        {
            var grammar = registry.LoadGrammar(scope);
            return grammar is null ? null : new Session(grammar, registry.GetTheme());
        }
    }

    /// <summary>Map common README code-fence spellings to bundled TextMate language IDs.</summary>
    public static string NormalizeLanguage(string language)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(language);
        return language.Trim().ToLowerInvariant() switch
        {
            "cs" or "c#" or "c-sharp" => "csharp",
            "sh" or "bash" or "zsh" or "shell" => "shellscript",
            "ps1" or "pwsh" => "powershell",
            "js" or "node" => "javascript",
            "ts" => "typescript",
            "py" => "python",
            "yml" => "yaml",
            "md" => "markdown",
            "c++" or "cc" => "cpp",
            "rs" => "rust",
            var other => other
        };
    }

    private sealed class Session(IGrammar grammar, Theme theme) : IMarkdownHighlightSession
    {
        private IStateStack? state;

        public IReadOnlyList<MarkdownHighlightSpan> HighlightLine(string codeLine)
        {
            ArgumentNullException.ThrowIfNull(codeLine);
            // TextMate keeps multiline grammar context through the rule stack.
            var result = grammar.TokenizeLine(codeLine, state, TimeSpan.FromMilliseconds(40));
            state = result.RuleStack;
            List<MarkdownHighlightSpan> spans = [];
            foreach (var token in result.Tokens)
            {
                var start = Math.Clamp(token.StartIndex, 0, codeLine.Length);
                var end = Math.Clamp(token.EndIndex, start, codeLine.Length);
                if (end == start)
                    continue;
                string? hex = null;
                foreach (var rule in theme.Match(token.Scopes))
                {
                    var candidate = theme.GetColor(rule.foreground);
                    if (!string.IsNullOrEmpty(candidate))
                        hex = candidate;
                }
                if (hex is { Length: 7 } && hex[0] == '#' &&
                    byte.TryParse(hex.AsSpan(1, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var r) &&
                    byte.TryParse(hex.AsSpan(3, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var g) &&
                    byte.TryParse(hex.AsSpan(5, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var b))
                    spans.Add(new MarkdownHighlightSpan(start, end - start,
                        new Style(foreground: new Color(r, g, b))));
            }
            return spans;
        }

        public void Dispose() => state = null;
    }
}
