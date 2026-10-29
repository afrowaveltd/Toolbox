using Afrowave.Toolbox.ExchangeHouse.Markdown;
using Spectre.Console;
using Spectre.Console.Testing;
using Xunit;

namespace Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown.Tests;

public sealed class MarkdownSpectreRendererTests
{
    private static TestConsole Console() => new();

    [Fact]
    public void Displays_headings_paragraphs_bold_italic_and_list_items()
    {
        var console = Console();
        MarkdownSpectreRenderer.Render(console, "# Heading\nSome **bold** and *italic* text.\n- First\n2. Second\n");
        Assert.Contains("Heading", console.Output);
        Assert.Contains("bold", console.Output);
        Assert.Contains("italic", console.Output);
        Assert.Contains("First", console.Output);
        Assert.Contains("Second", console.Output);
        Assert.DoesNotContain("**bold**", console.Output);
        Assert.DoesNotContain("# Heading", console.Output);
    }

    [Fact]
    public void Protects_code_block_and_does_not_interpret_spectre_markup()
    {
        var console = Console();
        MarkdownSpectreRenderer.Render(console, "```md\n# not heading\n[red]literal[/]\n```\n");
        Assert.Contains("# not heading", console.Output);
        Assert.Contains("[red]literal[/]", console.Output);
        Assert.DoesNotContain("```md", console.Output);
    }

    [Fact]
    public void Caller_can_customize_styles_bullets_destinations_and_code_indentation()
    {
        var theme = new MarkdownSpectreTheme
        {
            Heading1 = new Style(foreground: Color.Green, decoration: Decoration.Bold),
            ListBullet = "+",
            CodeIndent = "-> ",
            ShowCodeFenceDelimiters = true,
            ShowLinkDestinations = true
        };
        var document = MarkdownParser.Parse("# Demo\n- [Guide](../en.md)\n```\nhello\n```\n");
        var console = Console();
        MarkdownSpectreRenderer.Render(console, document, theme);
        Assert.Equal(theme.Heading1, theme.GetHeadingStyle(1));
        Assert.Contains("Guide", console.Output);
        Assert.Contains("../en.md", console.Output);
        Assert.Contains("-> hello", console.Output);
        Assert.Contains("```", console.Output);
        Assert.Equal(document.Source, MarkdownWriter.Write(document));
    }

    [Fact]
    public void Hides_link_destinations_by_default_and_reusing_document_does_not_mutate_it()
    {
        const string markdown = "See [guide](https://example.org/manual)\n";
        var document = MarkdownParser.Parse(markdown);
        var normal = Console();
        var verbose = Console();
        MarkdownSpectreRenderer.Render(normal, document);
        MarkdownSpectreRenderer.Render(verbose, document, new MarkdownSpectreTheme { ShowLinkDestinations = true });
        Assert.Contains("guide", normal.Output);
        Assert.DoesNotContain("example.org", normal.Output);
        Assert.Contains("example.org", verbose.Output);
        Assert.Equal(markdown, MarkdownWriter.Write(document));
    }

    [Fact]
    public void Terminal_escape_characters_are_never_forwarded_from_user_text()
    {
        var console = Console();
        MarkdownSpectreRenderer.Render(console, "Plain text \u001b[31mBAD\n");
        Assert.Contains("BAD", console.Output);
        // Spectre may emit its own ANSI styling; verify the untrusted ESC was replaced.
        Assert.Contains("\uFFFD[31mBAD", console.Output);
    }


    [Fact]
    public void Images_render_accessible_alt_text_without_opening_or_displaying_files()
    {
        const string source = "View ![Our **logo**](../img/logo.png \"Brand\") and [docs](../en.md)\n";
        var parsed = MarkdownParser.Parse(source);
        var console = Console();
        MarkdownSpectreRenderer.Render(console, parsed);

        Assert.Contains("[Image: Our logo]", console.Output);
        Assert.DoesNotContain("../img/logo.png", console.Output);
        Assert.DoesNotContain("Brand", console.Output);
        Assert.Contains("docs", console.Output);
        Assert.Equal(source, MarkdownWriter.Write(parsed));
    }

    [Fact]
    public void Image_label_and_original_destination_are_independently_themeable()
    {
        var theme = new MarkdownSpectreTheme
        {
            ImagePrefix = "Picture: ",
            ImageSuffix = "!",
            ShowImageDestinations = true,
            Image = new Style(foreground: Color.Yellow, decoration: Decoration.Bold)
        };
        var console = Console();
        MarkdownSpectreRenderer.Render(console, "![Logo](<../img/my logo.png> 'Brand')", theme);

        Assert.Contains("Picture: Logo! (../img/my logo.png)", console.Output);
        Assert.DoesNotContain("Brand", console.Output);
    }

    [Fact]
    public void Image_text_is_literal_and_cannot_inject_spectre_markup()
    {
        var console = Console();
        MarkdownSpectreRenderer.Render(console, "![danger [red]tag[/]](img.png)\n");
        Assert.Contains("[Image: danger [red]tag[/]]", console.Output);
    }

    [Fact]
    public void Rejects_null_input_and_invalid_heading_levels()
    {
        Assert.Throws<ArgumentNullException>(() => MarkdownSpectreRenderer.Render(null!, "x"));
        Assert.Throws<ArgumentNullException>(() => MarkdownSpectreRenderer.Render(Console(), (string)null!));
        Assert.Throws<ArgumentNullException>(() => MarkdownSpectreRenderer.Render(Console(), (MarkdownDocument)null!));
        Assert.Throws<ArgumentOutOfRangeException>(() => new MarkdownSpectreTheme().GetHeadingStyle(0));
    }
}
