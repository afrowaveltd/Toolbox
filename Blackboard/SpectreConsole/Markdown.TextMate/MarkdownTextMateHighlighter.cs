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
        // Direct language IDs cover the bundled registry; extension lookup allows
        // every additional bundled grammar to work without another hard-coded list.
        var scope = options.GetScopeByLanguageId(id);
        if (scope is null && id.Length <= 64 && id.All(character =>
            char.IsAsciiLetterOrDigit(character) || character is '_' or '-' or '+'))
        {
            var extensionLanguage = options.GetLanguageByExtension("." + id);
            if (extensionLanguage is not null)
                scope = options.GetScopeByLanguageId(extensionLanguage.Id);
        }
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
            // Names commonly found in README fences, command snippets and extensions.
            "cs" or "c#" or "c-sharp" or "csharp" or "dotnet" => "csharp",
            "vb" or "vbnet" or "visualbasic" or "visual-basic" => "vb",
            "f#" or "f-sharp" or "fs" => "fsharp",
            "c++" or "cxx" or "cc" or "hpp" or "hxx" => "cpp",
            "h" => "c",
            "sh" or "bash" or "zsh" or "ksh" or "shell" or "shell-script" or "shellscript" => "shellscript",
            "ps" or "ps1" or "pwsh" or "psm1" => "powershell",
            "js" or "node" or "nodejs" or "mjs" or "cjs" or "ecmascript" or "es6" => "javascript",
            "ts" or "mts" or "cts" => "typescript",
            "py" or "pyw" or "python3" => "python",
            "rb" or "ruby-script" => "ruby",
            "rs" => "rust",
            "golang" => "go",
            "kt" or "kts" => "kotlin",
            "swiftlang" => "swift",
            "dartlang" => "dart",
            "php3" or "php7" or "php8" => "php",
            "pl" or "perl5" => "perl",
            "lua-script" => "lua",
            "rscript" => "r",
            "sqlserver" or "tsql" or "mysql" or "postgres" or "postgresql" or "sqlite" => "sql",
            "htm" or "xhtml" => "html",
            "svg" or "xsd" or "xsl" or "xslt" or "csproj" or "fsproj" or "vbproj" => "xml",
            "scss" or "sass" => "scss",
            "lesscss" => "less",
            "jsn" or "jsonc" or "json5" or "jsonl" or "ndjson" or "ajis" => "json",
            "yml" or "yaml" => "yaml",
            "md" or "mdown" or "mkd" => "markdown",
            "tex" => "latex",
            "diff" or "patch" => "diff",
            "docker" or "dockerfile" => "dockerfile",
            "make" or "makefile" or "gnumake" => "makefile",
            "tf" or "terraform" => "terraform",
            "ini" or "cfg" or "conf" => "ini",
            "txt" or "plaintext" or "plain-text" => "text",
            var other => other // Direct registry ID or extension fallback, never whitelist-only.
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
