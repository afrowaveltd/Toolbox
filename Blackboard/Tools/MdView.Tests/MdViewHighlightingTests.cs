using Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown;
using Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown.TextMate;
using Afrowave.Toolbox.Blackboard.Tools.MdView;
using Spectre.Console.Testing;
using Xunit;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView.Tests;

public sealed class MdViewHighlightingTests
{
    [Fact]
    public void MdViewThemeEnablesBundledTextMateHighlighter()
    {
        var theme = MdViewHighlighting.CreateTheme();
        Assert.IsType<MarkdownTextMateHighlighter>(theme.SyntaxHighlighter);
    }

    [Fact]
    public void MdViewThemeRendersJavascriptAsSafeLiteralText()
    {
        var theme = MdViewHighlighting.CreateTheme();
        Assert.NotNull(theme.SyntaxHighlighter);
        using var session = theme.SyntaxHighlighter!.CreateSession("javascript");
        Assert.NotNull(session);
        Assert.NotEmpty(session!.HighlightLine("const answer = 42;"));

        var console = new TestConsole();
        MarkdownSpectreRenderer.Render(console,
            "```javascript\nconst answer = 42;\nconst literal = '[red]not markup[/]';\n```",
            theme);

        Assert.Contains("const answer = 42;", console.Output);
        Assert.Contains("[red]not markup[/]", console.Output);
    }

    [Fact]
    public void CodeFenceWithUnknownGrammarRemainsReadable()
    {
        var console = new TestConsole();
        MarkdownSpectreRenderer.Render(console,
            "```not-a-known-language\n<hello> & [red]literal[/]\n```",
            MdViewHighlighting.CreateTheme());

        Assert.Contains("<hello> & [red]literal[/]", console.Output);
    }
}
