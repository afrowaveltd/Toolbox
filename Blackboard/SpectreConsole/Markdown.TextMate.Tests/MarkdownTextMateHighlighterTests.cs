using Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown;
using Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown.TextMate;
using Spectre.Console.Testing;
using Xunit;

namespace Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown.TextMate.Tests;

public sealed class MarkdownTextMateHighlighterTests
{
    [Theory]
    [InlineData("CS", "csharp")]
    [InlineData("bash", "shellscript")]
    [InlineData("pwsh", "powershell")]
    [InlineData("ts", "typescript")]
    public void Aliases_are_normalized(string hint, string language) =>
        Assert.Equal(language, MarkdownTextMateHighlighter.NormalizeLanguage(hint));

    [Theory]
    [InlineData("JS", "javascript")]
    [InlineData("nodejs", "javascript")]
    [InlineData("mjs", "javascript")]
    [InlineData("ts", "typescript")]
    [InlineData("py", "python")]
    [InlineData("c++", "cpp")]
    [InlineData("rs", "rust")]
    [InlineData("golang", "go")]
    [InlineData("sqlserver", "sql")]
    [InlineData("jsonc", "json")]
    [InlineData("ndjson", "json")]
    [InlineData("ajis", "json")]
    [InlineData("yml", "yaml")]
    [InlineData("xhtml", "html")]
    [InlineData("svg", "xml")]
    [InlineData("docker", "dockerfile")]
    [InlineData("patch", "diff")]
    [InlineData("PS1", "powershell")]
    [InlineData("sh", "shellscript")]
    [InlineData("txt", "text")]
    public void Common_fence_aliases_resolve_without_case_sensitivity(string hint, string expected) =>
        Assert.Equal(expected, MarkdownTextMateHighlighter.NormalizeLanguage(hint));

    [Theory]
    [InlineData("js", "const greet = (name) => 'Hello ' + name;")]
    [InlineData("javascript", "function greet(name) { return name; }")]
    [InlineData("ts", "interface Person { name: string; }")]
    [InlineData("python", "def greeting(name): return name")]
    [InlineData("json", "{ \"name\": \"Afrowave\", \"version\": 2 }")]
    [InlineData("yaml", "name: Afrowave")]
    [InlineData("html", "<section class=\"main\">Hello</section>")]
    [InlineData("css", ".main { color: red; }")]
    [InlineData("sql", "SELECT name FROM users WHERE id = 1;")]
    [InlineData("go", "func main() { println(\"Hello\") }")]
    [InlineData("rust", "fn main() { println!(\"Hello\"); }")]
    [InlineData("java", "public class Demo { int count = 2; }")]
    [InlineData("cpp", "int main() { return 0; }")]
    [InlineData("powershell", "Write-Host \"Hello\"")]
    [InlineData("markdown", "# Heading")]
    public void Bundled_programming_and_document_grammars_are_selected(string hint, string source)
    {
        var highlighter = new MarkdownTextMateHighlighter();
        using var session = highlighter.CreateSession(hint);
        Assert.NotNull(session);
        var spans = session!.HighlightLine(source);
        Assert.All(spans, span =>
        {
            Assert.InRange(span.Start, 0, source.Length - 1);
            Assert.InRange(span.Length, 1, source.Length - span.Start);
        });
    }

    [Fact]
    public void Javascript_is_actually_highlighted_and_markup_is_still_literal()
    {
        var highlighter = new MarkdownTextMateHighlighter();
        using var session = highlighter.CreateSession("js");
        Assert.NotNull(session);
        const string code = "const content = '[red]literal[/]';";
        Assert.NotEmpty(session!.HighlightLine(code));

        var console = new TestConsole();
        MarkdownSpectreRenderer.Render(console,
            "```javascript\nconst content = '[red]literal[/]';\n```",
            new MarkdownSpectreTheme { SyntaxHighlighter = highlighter });
        Assert.Contains("[red]literal[/]", console.Output);
    }

    [Fact]
    public void Language_without_bundled_grammar_does_not_crash_or_evaluate_markup()
    {
        var highlighter = new MarkdownTextMateHighlighter();
        Assert.Null(highlighter.CreateSession("unregistered-language-afrowave-999"));
        var console = new TestConsole();
        MarkdownSpectreRenderer.Render(console,
            "```unregistered-language-afrowave-999\n[red]literal[/]\n```",
            new MarkdownSpectreTheme { SyntaxHighlighter = highlighter });
        Assert.Contains("[red]literal[/]", console.Output);
    }

    [Fact]
    public void Csharp_grammar_produces_real_styled_tokens()
    {
        var highlighter = new MarkdownTextMateHighlighter();
        using var session = highlighter.CreateSession("cs");
        Assert.NotNull(session);
        const string source = "public class Demo {}";
        var spans = session!.HighlightLine(source);
        Assert.NotEmpty(spans);
        Assert.All(spans, span =>
        {
            Assert.InRange(span.Start, 0, source.Length - 1);
            Assert.InRange(span.Length, 1, source.Length - span.Start);
        });
    }

    [Fact]
    public void Unknown_language_falls_back_to_safe_plain_text()
    {
        var highlighter = new MarkdownTextMateHighlighter();
        Assert.Null(highlighter.CreateSession("unknown-afrowave-123"));
        var console = new TestConsole();
        MarkdownSpectreRenderer.Render(console,
            "```cs\npublic string Example = \"[red]literal[/]\";\n```",
            new MarkdownSpectreTheme { SyntaxHighlighter = highlighter });
        Assert.Contains("[red]literal[/]", console.Output);
    }
}
