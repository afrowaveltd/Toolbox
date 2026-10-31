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
