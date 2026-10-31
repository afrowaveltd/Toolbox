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
    public void Mixed_nested_lists_keep_real_ordered_numbers_and_indentation()
    {
        const string source = "- parent\n  7) first\n    * **detail**\n  8. second\n- end\n";
        var doc = MarkdownParser.Parse(source);
        var console = Console();
        MarkdownSpectreRenderer.Render(console, doc);

        Assert.Contains("• parent", console.Output);
        Assert.Contains("  7) first", console.Output);
        Assert.Contains("    • detail", console.Output);
        Assert.Contains("  8. second", console.Output);
        Assert.Contains("• end", console.Output);
        Assert.Equal(source, MarkdownWriter.Write(doc));
        Assert.Equal(2, doc.ListRoots.Count);
    }

    [Fact]
    public void List_theme_can_preserve_original_unordered_markers_and_change_indent()
    {
        var console = Console();
        var theme = new MarkdownSpectreTheme
        {
            PreserveUnorderedMarkers = true,
            ListIndent = "....",
            ListBullet = "unused"
        };
        MarkdownSpectreRenderer.Render(console, "+ root\n  - child\n    * grandchild\n", theme);

        Assert.Contains("+ root", console.Output);
        Assert.Contains("....- child", console.Output);
        Assert.Contains("........* grandchild", console.Output);
        Assert.DoesNotContain("unused", console.Output);
    }

    [Fact]
    public void Ordered_markers_are_not_replaced_by_configured_unordered_bullet()
    {
        var console = Console();
        MarkdownSpectreRenderer.Render(console, "14) start\n15. next\n- plain\n",
            new MarkdownSpectreTheme { ListBullet = "+" });

        Assert.Contains("14) start", console.Output);
        Assert.Contains("15. next", console.Output);
        Assert.Contains("+ plain", console.Output);
    }

    [Fact]
    public void Fenced_code_is_not_treated_as_nested_list()
    {
        var console = Console();
        MarkdownSpectreRenderer.Render(console,
            "- root\n```md\n  42) literally code\n```\n- new root\n");
        Assert.Contains("    42) literally code", console.Output);
        Assert.DoesNotContain("    • literally code", console.Output);
    }


    [Fact]
    public void Indented_continuations_align_with_list_content_without_new_bullets()
    {
        const string markdown = "- First\n  Continued **strong** text\n  [Manual](../manual.md)\n- Second";
        var doc = MarkdownParser.Parse(markdown);
        var console = Console();
        MarkdownSpectreRenderer.Render(console, doc, new MarkdownSpectreTheme { ShowLinkDestinations = true });

        Assert.Contains("• First", console.Output);
        Assert.Contains("  Continued strong text", console.Output);
        Assert.Contains("  Manual (../manual.md)", console.Output);
        Assert.Contains("• Second", console.Output);
        Assert.DoesNotContain("• Continued", console.Output);
        Assert.Equal(markdown, MarkdownWriter.Write(doc));
    }

    [Fact]
    public void Ordered_and_nested_continuations_follow_marker_width_and_theme_indent()
    {
        const string markdown = "12) Numbered\n    Next line\n    - Child\n      Child detail\n    Parent detail";
        var doc = MarkdownParser.Parse(markdown);
        var console = Console();
        MarkdownSpectreRenderer.Render(console, doc, new MarkdownSpectreTheme
        {
            ListIndent = "....", ListBullet = "+"
        });

        Assert.Contains("12) Numbered", console.Output);
        Assert.Contains("    Next line", console.Output);
        Assert.Contains("....+ Child", console.Output);
        Assert.Contains("....  Child detail", console.Output);
        Assert.Contains("    Parent detail", console.Output);
        Assert.Equal(markdown, MarkdownWriter.Write(doc));
    }

    [Fact]
    public void Blank_and_unindented_paragraphs_remain_separate_from_list_content()
    {
        const string markdown = "- Item\n  attached\n\nOutside\n  still outside";
        var doc = MarkdownParser.Parse(markdown);
        var console = Console();
        MarkdownSpectreRenderer.Render(console, doc);
        Assert.Contains("  attached", console.Output);
        Assert.Contains("Outside\n", console.Output);
        Assert.Contains("still outside", console.Output);
        Assert.Null(doc.Blocks[4].ListContinuationMarker);
    }


    [Fact]
    public void Loose_list_item_has_blank_separator_and_paragraph_without_repeated_bullet()
    {
        const string markdown = "- First\n  Intro\n\n  **Later** [guide](../guide.md)\n- Second";
        var doc = MarkdownParser.Parse(markdown);
        var console = Console();
        MarkdownSpectreRenderer.Render(console, doc,
            new MarkdownSpectreTheme { ShowLinkDestinations = true });
        Assert.True(doc.ListRoots[0].IsLoose);
        Assert.Contains("• First\n  Intro\n\n  Later guide (../guide.md)", console.Output);
        Assert.Contains("• Second", console.Output);
        Assert.DoesNotContain("• Later", console.Output);
        Assert.Equal(markdown, MarkdownWriter.Write(doc));
    }

    [Fact]
    public void Loose_nested_numbered_item_uses_original_number_and_custom_theme_indent()
    {
        const string markdown = "- Root\n  12) Numbered\n\n      After gap\n- End";
        var doc = MarkdownParser.Parse(markdown);
        var theme = new MarkdownSpectreTheme { ListIndent = "....", ListBullet = "+" };
        var console = Console();
        MarkdownSpectreRenderer.Render(console, doc, theme);
        var numbered = Assert.Single(doc.ListRoots[0].Children);
        Assert.True(numbered.IsLoose);
        Assert.Contains("....12) Numbered\n\n....    After gap", console.Output);
        Assert.Contains("+ End", console.Output);
        Assert.Equal(markdown, MarkdownWriter.Write(doc));
    }

    [Fact]
    public void Blank_before_unrelated_paragraph_does_not_create_loose_item()
    {
        const string markdown = "- Item\n\nOutside";
        var doc = MarkdownParser.Parse(markdown);
        var console = Console();
        MarkdownSpectreRenderer.Render(console, doc);
        Assert.False(Assert.Single(doc.ListRoots).IsLoose);
        Assert.Contains("• Item\n\nOutside", console.Output);
        Assert.Null(doc.Blocks[1].ListBlankOwnerStart);
    }


    [Fact]
    public void Lazy_paragraphs_align_with_their_bullet_and_do_not_create_extra_markers()
    {
        const string source = "- First\nLazy **text**\n- Second";
        var doc = MarkdownParser.Parse(source);
        var console = Console();
        MarkdownSpectreRenderer.Render(console, doc);
        Assert.True(doc.Blocks[1].IsLazyListContinuation);
        Assert.Contains("• First\n  Lazy text\n• Second", console.Output);
        Assert.Equal(source, MarkdownWriter.Write(doc));
    }

    [Fact]
    public void Lazy_nested_numbered_content_uses_the_original_marker_width()
    {
        const string source = "- Parent\n  12) Child\nLazy child detail\n  Parent detail";
        var doc = MarkdownParser.Parse(source);
        var console = Console();
        MarkdownSpectreRenderer.Render(console, doc,
            new MarkdownSpectreTheme { ListIndent = "....", ListBullet = "+" });
        Assert.Contains("....12) Child\n....    Lazy child detail", console.Output);
        Assert.Contains("  Parent detail", console.Output);
        Assert.Equal(source, MarkdownWriter.Write(doc));
    }


    [Fact]
    public void Indented_code_and_quotes_render_inside_ordered_list_without_source_markup()
    {
        const string markdown = "- Parent\n  12) Code\n      \u0060\u0060\u0060bash\n      echo '[red]literal[/]'\n      \u0060\u0060\u0060\n      > **Important**\n- End";
        var console = Console();
        MarkdownSpectreRenderer.Render(console, markdown, new MarkdownSpectreTheme
        {
            ListIndent = "....", ListBullet = "+", ShowCodeLanguage = true
        });
        Assert.Contains("....12) Code", console.Output);
        Assert.Contains("....        [bash]", console.Output);
        Assert.Contains("....        echo '[red]literal[/]'", console.Output);
        Assert.Contains("....    │ Important", console.Output);
        Assert.DoesNotContain("**Important**", console.Output);
        Assert.Contains("+ End", console.Output);
    }

    [Fact]
    public void Quote_levels_are_rendered_as_safe_literal_text()
    {
        var console = Console();
        MarkdownSpectreRenderer.Render(console, "> one\n>> **two**\n", new MarkdownSpectreTheme
        {
            QuotePrefix = "> "
        });
        Assert.Contains("> one", console.Output);
        Assert.Contains("> > two", console.Output);
        Assert.DoesNotContain("**two**", console.Output);
    }

    [Fact]
    public void External_highlighter_sessions_are_scoped_to_each_fence_and_source_is_unchanged()
    {
        const string markdown = "\u0060\u0060\u0060csharp\npublic class Hello {}\n\u0060\u0060\u0060\n\u0060\u0060\u0060unknown\nliteral\n\u0060\u0060\u0060";
        var backend = new TestHighlighter();
        var parsed = MarkdownParser.Parse(markdown);
        var console = Console();
        MarkdownSpectreRenderer.Render(console, parsed, new MarkdownSpectreTheme
        {
            SyntaxHighlighter = backend
        });
        Assert.Contains("public class Hello {}", console.Output);
        Assert.Contains("literal", console.Output);
        Assert.Equal(new[] { "csharp", "unknown" }, backend.Requests);
        Assert.Equal(1, backend.Disposals);
        Assert.Equal(markdown, MarkdownWriter.Write(parsed));
    }

    private sealed class TestHighlighter : IMarkdownSyntaxHighlighter
    {
        public List<string> Requests { get; } = [];
        public int Disposals { get; private set; }
        public IMarkdownHighlightSession? CreateSession(string language)
        {
            Requests.Add(language);
            return language == "csharp" ? new Session(this) : null;
        }

        private sealed class Session(TestHighlighter owner) : IMarkdownHighlightSession
        {
            public IReadOnlyList<MarkdownHighlightSpan> HighlightLine(string line) =>
                line.StartsWith("public", StringComparison.Ordinal)
                    ? [new MarkdownHighlightSpan(0, 6, new Style(foreground: Color.Green))]
                    : [];
            public void Dispose() => owner.Disposals++;
        }
    }


    [Fact]
    public void Nested_quoted_lists_render_quote_levels_and_original_ordered_markers()
    {
        const string source = "> - Parent\n>   12) **Child**\n>     * Detail\n>   13) Next\n>> - Other quote level\n- Outside";
        var console = Console();
        MarkdownSpectreRenderer.Render(console, source, new MarkdownSpectreTheme
        {
            QuotePrefix = "| ", ListIndent = "....", ListBullet = "+"
        });
        Assert.Contains("| + Parent", console.Output);
        Assert.Contains("| ....12) Child", console.Output);
        Assert.Contains("| ........+ Detail", console.Output);
        Assert.Contains("| ....13) Next", console.Output);
        Assert.Contains("| | + Other quote level", console.Output);
        Assert.Contains("+ Outside", console.Output);
        Assert.DoesNotContain("**Child**", console.Output);
    }

    [Fact]
    public void Quoted_lazy_and_explicit_paragraphs_align_under_quoted_item_marker()
    {
        const string source = "> 12) First\n>     Explicit **strong**\n> Lazy [guide](../guide.md)\n> 13) Second";
        var doc = MarkdownParser.Parse(source);
        var console = Console();
        MarkdownSpectreRenderer.Render(console, doc, new MarkdownSpectreTheme
        {
            QuotePrefix = "> ", ShowLinkDestinations = true
        });
        Assert.Contains("> 12) First", console.Output);
        Assert.Contains(">     Explicit strong", console.Output);
        Assert.Contains(">     Lazy guide (../guide.md)", console.Output);
        Assert.Contains("> 13) Second", console.Output);
        Assert.Equal(source, MarkdownWriter.Write(doc));
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
