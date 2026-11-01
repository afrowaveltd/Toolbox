using Xunit;

namespace Afrowave.Toolbox.ExchangeHouse.Markdown.Tests;

public sealed class MarkdownParserTests
{
    [Theory]
    [InlineData("")]
    [InlineData("# Hello\n\n- item\n")]
    [InlineData("# Heading\r\nA paragraph  \r\n- item")]
    [InlineData("one\rtwo\rthree")]
    [InlineData("   ## Česky 🐈\n~~end~~\n")]
    [InlineData("```csharp\n# not a heading\n- not a list\n```\n")]
    [InlineData("    # not a heading\n####### not a heading\n")]
    public void Parse_and_write_preserve_every_character(string source)
    {
        var document = MarkdownParser.Parse(source);

        Assert.Equal(source, MarkdownWriter.Write(document));
        var offset = 0;
        foreach (var block in document.Blocks)
        {
            Assert.Equal(offset, block.Start);
            Assert.Equal(block.RawText.Length, block.Length);
            Assert.Equal(source.Substring(block.Start, block.Length), block.RawText);
            offset += block.Length;
        }

        Assert.Equal(source.Length, offset);
    }

    [Fact]
    public void Headings_and_lists_are_recognized_without_reinterpreting_code()
    {
        const string source = "# Heading\n- item\n1. second\n~~~\n# protected\n- protected\n~~~\n";
        var blocks = MarkdownParser.Parse(source).Blocks;
        Assert.Equal(new[]
        {
            MarkdownBlockKind.Heading,
            MarkdownBlockKind.ListItem,
            MarkdownBlockKind.ListItem,
            MarkdownBlockKind.CodeFence,
            MarkdownBlockKind.CodeLine,
            MarkdownBlockKind.CodeLine,
            MarkdownBlockKind.CodeFence
        }, blocks.Select(x => x.Kind));
        Assert.Equal(1, blocks[0].HeadingLevel);
        Assert.Equal("Heading", blocks[0].Text);
        Assert.Equal("item", blocks[1].Text);
        Assert.Equal("# protected", blocks[4].Text);
    }

    [Fact]
    public void Fence_must_close_with_matching_marker_and_length()
    {
        const string source = "````csharp\n~~~\n```\n````\n# outside\n";
        var blocks = MarkdownParser.Parse(source).Blocks;
        Assert.Equal(MarkdownBlockKind.CodeLine, blocks[1].Kind);
        Assert.Equal(MarkdownBlockKind.CodeLine, blocks[2].Kind);
        Assert.Equal(MarkdownBlockKind.CodeFence, blocks[3].Kind);
        Assert.Equal(MarkdownBlockKind.Heading, blocks[4].Kind);
        Assert.Equal(source, MarkdownWriter.Write(MarkdownParser.Parse(source)));
    }

    [Fact]
    public void Only_valid_heading_markers_are_classified()
    {
        var blocks = MarkdownParser.Parse("#good\n####### too deep\n   #### good\n    # indented\n").Blocks;
        Assert.Equal(MarkdownBlockKind.Paragraph, blocks[0].Kind);
        Assert.Equal(MarkdownBlockKind.Paragraph, blocks[1].Kind);
        Assert.Equal(MarkdownBlockKind.Heading, blocks[2].Kind);
        Assert.Equal(4, blocks[2].HeadingLevel);
        Assert.Equal(MarkdownBlockKind.Paragraph, blocks[3].Kind);
    }


    [Fact]
    public void Nested_and_mixed_lists_expose_depth_marker_and_parent_child_hierarchy()
    {
        const string source = "- parent\r\n  7) step\n    * **nested**\n  8) next\n- end";
        var document = MarkdownParser.Parse(source);
        Assert.Equal(source, MarkdownWriter.Write(document));
        Assert.Equal(5, document.Blocks.Count);
        Assert.Equal(new[] { 0, 1, 2, 1, 0 },
            document.Blocks.Select(b => b.ListMarker!.Depth));
        Assert.Equal(new[] { "-", "7)", "*", "8)", "-" },
            document.Blocks.Select(b => b.ListMarker!.OriginalMarker));
        Assert.Equal(7, document.Blocks[1].ListMarker!.Number);
        Assert.Equal(')', document.Blocks[1].ListMarker!.Delimiter);
        Assert.Equal(8, document.Blocks[3].ListMarker!.Number);
        Assert.Equal(2, document.ListRoots.Count);
        Assert.Equal(2, document.ListRoots[0].Children.Count);
        Assert.Single(document.ListRoots[0].Children[0].Children);
        Assert.Equal("nested", Assert.Single(document.Blocks[2].Inlines,
            n => n.Kind == MarkdownInlineKind.Strong).Text);
        foreach (var block in document.Blocks)
        {
            var marker = block.ListMarker!;
            Assert.Equal(block.Text, source[marker.ContentStart..(block.Start + block.Length)]
                .TrimEnd('\r', '\n'));
        }
    }

    [Fact]
    public void Tabs_are_measured_as_columns_but_raw_source_spans_are_unchanged()
    {
        const string source = "1. first\r\n\t- child\r\n\t\t2. grandchild\r\n2. second";
        var document = MarkdownParser.Parse(source);
        Assert.Equal(new[] { 0, 1, 2, 0 },
            document.Blocks.Select(b => b.ListMarker!.Depth));
        Assert.Equal(new[] { 0, 4, 8, 0 },
            document.Blocks.Select(b => b.ListMarker!.IndentColumns));
        Assert.Equal(source, MarkdownWriter.Write(document));
    }

    [Fact]
    public void Non_list_lines_and_fences_break_the_previous_list_hierarchy()
    {
        const string source = "- first\n  - child\n\n  - separate\nplain\n  - another\n```\n  - protected\n```\n- final";
        var doc = MarkdownParser.Parse(source);
        // Blank/paragraph/fence boundaries create four separate list runs.
        Assert.Equal(4, doc.ListRoots.Count);
        Assert.Single(doc.ListRoots[0].Children);
        Assert.Equal(MarkdownBlockKind.ListItem, doc.Blocks[3].Kind);
        Assert.Equal(0, doc.Blocks[3].ListMarker!.Depth);
        Assert.Equal(MarkdownBlockKind.CodeLine, doc.Blocks[7].Kind);
        Assert.Null(doc.Blocks[7].ListMarker);
        Assert.Equal(source, MarkdownWriter.Write(doc));
    }

    [Theory]
    [InlineData("0000000001. too many digits")]
    [InlineData("-not a marker")]
    [InlineData("1.not a marker")]
    [InlineData("    - not a root list item")]
    public void Unsupported_or_indented_root_markers_remain_plain_text(string input)
    {
        var document = MarkdownParser.Parse(input);
        Assert.Equal(MarkdownBlockKind.Paragraph, Assert.Single(document.Blocks).Kind);
        Assert.Empty(document.ListRoots);
        Assert.Equal(input, MarkdownWriter.Write(document));
    }

    [Fact]
    public void Inline_nodes_keep_absolute_offsets_after_indented_list_marker()
    {
        const string markdown = "- parent\n    12. [Manual](../Docs/en.md) and `code`";
        var doc = MarkdownParser.Parse(markdown);
        var nested = doc.Blocks[1];
        Assert.Equal("12.", nested.ListMarker!.OriginalMarker);
        Assert.Equal(1, nested.ListMarker.Depth);
        Assert.Equal("[Manual](../Docs/en.md) and `code`", nested.Text);
        foreach (var node in nested.Inlines)
            Assert.Equal(node.RawText, markdown.Substring(node.Start, node.Length));
    }


    [Fact]
    public void Explicit_continuations_attach_to_item_with_original_offsets()
    {
        const string markdown = "- **Title**\r\n  Follow [the guide](../guide.md)\r\n  More information\r\n- Next\r\n";
        var doc = MarkdownParser.Parse(markdown);
        var first = doc.ListRoots[0];
        Assert.Equal(2, doc.ListRoots.Count);
        Assert.Equal(2, first.ContinuationBlocks.Count);
        Assert.Equal("Follow [the guide](../guide.md)", first.ContinuationBlocks[0].Text);
        Assert.Equal(first.Block.Start, first.ContinuationBlocks[0].ListContinuationOwnerStart);
        Assert.Equal(first.Block.ListMarker, first.ContinuationBlocks[1].ListContinuationMarker);
        Assert.Equal(2, first.Block.ListMarker!.ContentIndentColumns);
        var link = Assert.Single(first.ContinuationBlocks[0].Inlines, n => n.Kind == MarkdownInlineKind.Link);
        Assert.Equal("../guide.md", link.Destination);
        Assert.Equal(link.RawText, markdown.Substring(link.Start, link.Length));
        Assert.Equal(markdown, MarkdownWriter.Write(doc));
    }

    [Fact]
    public void Continuation_after_nested_child_can_belong_to_parent()
    {
        const string markdown = "- parent\n  - child\n    child text\n  parent text\n- sibling";
        var doc = MarkdownParser.Parse(markdown);
        var parent = doc.ListRoots[0];
        var child = Assert.Single(parent.Children);
        Assert.Equal("child text", Assert.Single(child.ContinuationBlocks).Text);
        Assert.Equal("parent text", Assert.Single(parent.ContinuationBlocks).Text);
        Assert.Equal(parent.Block.Start, parent.ContinuationBlocks[0].ListContinuationOwnerStart);
        Assert.Equal(2, doc.ListRoots.Count);
        Assert.Equal(markdown, MarkdownWriter.Write(doc));
    }

    [Fact]
    public void Ordered_markers_and_tab_stops_control_continuation_columns()
    {
        const string markdown = "12) numbered\r\n    line two\r\n\tline three\r\n13. next";
        var doc = MarkdownParser.Parse(markdown);
        Assert.Equal(4, doc.ListRoots[0].Block.ListMarker!.ContentIndentColumns);
        Assert.Equal(2, doc.ListRoots[0].ContinuationBlocks.Count);
        Assert.Equal("    line two\r\n", doc.ListRoots[0].ContinuationBlocks[0].RawText);
        Assert.Equal("\tline three\r\n", doc.ListRoots[0].ContinuationBlocks[1].RawText);
        Assert.Equal(2, doc.ListRoots.Count);
        Assert.Equal(markdown, MarkdownWriter.Write(doc));
    }

    [Fact]
    public void Blank_and_unindented_paragraphs_end_list_continuations()
    {
        const string markdown = "- first\n  attached\n\nnot attached\n- second\n\nplain\n  also outside";
        var doc = MarkdownParser.Parse(markdown);
        Assert.Equal(2, doc.ListRoots.Count);
        Assert.Single(doc.ListRoots[0].ContinuationBlocks);
        Assert.Empty(doc.ListRoots[1].ContinuationBlocks);
        Assert.All(doc.Blocks.Where(b => b.Text.Contains("outside") || b.Text.Contains("not attached")),
            b => Assert.Null(b.ListContinuationMarker));
        Assert.Equal(markdown, MarkdownWriter.Write(doc));
    }

    [Fact]
    public void Top_level_fences_do_not_attach_to_list_content()
    {
        const string markdown = "- first\n  continuation\n~~~md\n  [literal](not-a-link)\n~~~\n- second";
        var doc = MarkdownParser.Parse(markdown);
        Assert.Single(doc.ListRoots[0].ContinuationBlocks);
        Assert.Equal(MarkdownBlockKind.CodeFence, doc.Blocks[2].Kind);
        Assert.Equal(MarkdownBlockKind.CodeLine, doc.Blocks[3].Kind);
        Assert.Null(doc.Blocks[3].ListContinuationMarker);
        Assert.Empty(doc.Blocks[3].Inlines);
        Assert.Equal(markdown, MarkdownWriter.Write(doc));
    }


    [Fact]
    public void Loose_item_owns_blank_separator_and_later_indented_paragraphs()
    {
        const string source = "- First\r\n  Initial **text**\r\n\r\n  Another [guide](../Docs/en.md)\r\n- Second\r\n";
        var doc = MarkdownParser.Parse(source);
        var item = doc.ListRoots[0];
        Assert.True(item.IsLoose);
        Assert.Single(item.SeparatorBlocks);
        Assert.Equal(2, item.ContinuationBlocks.Count);
        Assert.Equal(3, item.ContentBlocks.Count);
        Assert.Equal(new[] { "  Initial **text**\r\n", "\r\n", "  Another [guide](../Docs/en.md)\r\n" },
            item.ContentBlocks.Select(block => block.RawText));
        var separator = item.SeparatorBlocks[0];
        Assert.Equal(MarkdownBlockKind.Blank, separator.Kind);
        Assert.Equal(item.Block.Start, separator.ListBlankOwnerStart);
        Assert.Equal(separator, doc.Blocks[2]);
        var link = Assert.Single(item.ContinuationBlocks[1].Inlines,
            n => n.Kind == MarkdownInlineKind.Link);
        Assert.Equal(link.RawText, source.Substring(link.Start, link.Length));
        Assert.Equal(source, MarkdownWriter.Write(doc));
    }

    [Fact]
    public void Multiple_blanks_and_tabs_preserve_positions_and_source_exactly()
    {
        const string source = "12) Entry\r\n\r\n \t\r\n\tMore details\r\n";
        var doc = MarkdownParser.Parse(source);
        var item = Assert.Single(doc.ListRoots);
        Assert.Equal(4, item.Block.ListMarker!.ContentIndentColumns);
        Assert.Equal(2, item.SeparatorBlocks.Count);
        Assert.Equal(3, item.ContentBlocks.Count);
        Assert.Equal("More details", Assert.Single(item.ContinuationBlocks).Text);
        Assert.All(item.SeparatorBlocks, b => Assert.Equal(item.Block.Start, b.ListBlankOwnerStart));
        Assert.Equal(source, MarkdownWriter.Write(doc));
        foreach (var block in doc.Blocks)
            Assert.Equal(block.RawText, source.Substring(block.Start, block.Length));
    }

    [Fact]
    public void Blank_after_nested_child_reattaches_to_eligible_ancestor()
    {
        const string source = "- Parent\n  - Child\n    Child text\n\n  Parent after gap\n- End";
        var doc = MarkdownParser.Parse(source);
        var parent = doc.ListRoots[0];
        var child = Assert.Single(parent.Children);
        Assert.False(child.IsLoose);
        Assert.Equal("Child text", Assert.Single(child.ContinuationBlocks).Text);
        Assert.True(parent.IsLoose);
        Assert.Single(parent.SeparatorBlocks);
        Assert.Equal("Parent after gap", Assert.Single(parent.ContinuationBlocks).Text);
        Assert.Equal(parent.Block.Start, doc.Blocks[3].ListBlankOwnerStart);
        Assert.Equal(parent.Block.Start, doc.Blocks[4].ListContinuationOwnerStart);
        Assert.Equal(source, MarkdownWriter.Write(doc));
    }

    [Theory]
    [InlineData("- First\n\n")]
    [InlineData("- First\n\nUnindented text")]
    [InlineData("- First\n\n  - New child")]
    [InlineData("- First\n\n   ")]
    public void Trailing_or_unrelated_blank_lines_are_not_silently_owned(string source)
    {
        var doc = MarkdownParser.Parse(source);
        Assert.NotEmpty(doc.ListRoots);
        var first = doc.ListRoots[0];
        Assert.Empty(first.SeparatorBlocks);
        Assert.False(first.IsLoose);
        Assert.DoesNotContain(doc.Blocks, b => b.ListBlankOwnerStart.HasValue);
        Assert.Equal(source, MarkdownWriter.Write(doc));
    }

    [Fact]
    public void Blank_before_an_unrelated_marker_must_not_join_lists()
    {
        const string source = "- First\n\n  - Child-looking marker\n- Second";
        var doc = MarkdownParser.Parse(source);
        Assert.Equal(3, doc.ListRoots.Count);
        Assert.All(doc.ListRoots, item => Assert.False(item.IsLoose));
        Assert.Equal(0, doc.Blocks[2].ListMarker!.Depth);
        Assert.Equal(source, MarkdownWriter.Write(doc));
    }

    [Fact]
    public void Loose_list_syntax_inside_fences_is_not_interpreted()
    {
        const string source = "- First\n\n  Included\n~~~md\n- protected\n\n  protected\n~~~\n- Second";
        var doc = MarkdownParser.Parse(source);
        Assert.True(doc.ListRoots[0].IsLoose);
        Assert.Equal(MarkdownBlockKind.CodeLine, doc.Blocks[4].Kind);
        Assert.Equal(MarkdownBlockKind.CodeLine, doc.Blocks[5].Kind);
        Assert.Empty(doc.Blocks[4].Inlines);
        Assert.Null(doc.Blocks[5].ListBlankOwnerStart);
        Assert.Equal(source, MarkdownWriter.Write(doc));
    }


    [Fact]
    public void Lazy_paragraph_lines_attach_without_indent_and_keep_inline_offsets()
    {
        const string source = "- First\r\nContinues with **bold**\r\nMore [docs](../docs/en.md)\r\n- Second";
        var doc = MarkdownParser.Parse(source);
        var first = doc.ListRoots[0];
        Assert.Equal(2, doc.ListRoots.Count);
        Assert.Equal(2, first.ContinuationBlocks.Count);
        Assert.All(first.ContinuationBlocks, b =>
        {
            Assert.True(b.IsLazyListContinuation);
            Assert.Equal(first.Block.Start, b.ListContinuationOwnerStart);
            Assert.Equal(b.RawText, source.Substring(b.Start, b.Length));
        });
        Assert.Equal("Continues with **bold**", first.ContinuationBlocks[0].Text);
        var link = Assert.Single(first.ContinuationBlocks[1].Inlines,
            n => n.Kind == MarkdownInlineKind.Link);
        Assert.Equal("../docs/en.md", link.Destination);
        Assert.Equal(link.RawText, source.Substring(link.Start, link.Length));
        Assert.Equal(source, MarkdownWriter.Write(doc));
    }

    [Fact]
    public void Nested_lazy_line_belongs_to_current_child_until_parent_resumes()
    {
        const string source = "- Parent\n  12) Child\nChild is still here\n  Parent resumes\n- Next";
        var doc = MarkdownParser.Parse(source);
        var parent = doc.ListRoots[0];
        var child = Assert.Single(parent.Children);
        var lazy = Assert.Single(child.ContinuationBlocks);
        Assert.True(lazy.IsLazyListContinuation);
        Assert.Equal(child.Block.Start, lazy.ListContinuationOwnerStart);
        var explicitLine = Assert.Single(parent.ContinuationBlocks);
        Assert.False(explicitLine.IsLazyListContinuation);
        Assert.Equal("Parent resumes", explicitLine.Text);
        Assert.Equal(source, MarkdownWriter.Write(doc));
    }

    [Fact]
    public void Blank_before_unindented_text_prevents_lazy_ownership()
    {
        const string source = "- First\nFirst continues\n\nOutside\n- Second\n\nStill outside";
        var doc = MarkdownParser.Parse(source);
        Assert.Single(doc.ListRoots[0].ContinuationBlocks);
        Assert.True(doc.Blocks[1].IsLazyListContinuation);
        Assert.Null(doc.Blocks[3].ListContinuationOwnerStart);
        Assert.Null(doc.Blocks[6].ListContinuationOwnerStart);
        Assert.All(doc.ListRoots, item => Assert.False(item.IsLoose));
        Assert.Equal(source, MarkdownWriter.Write(doc));
    }

    [Fact]
    public void Heading_fence_quote_and_thematic_break_interrupt_lazy_paragraphs()
    {
        const string source = "- A\n# Heading\n- B\n~~~md\nprotected\n~~~\n- C\n> quote\n- D\n---\n- E";
        var doc = MarkdownParser.Parse(source);
        Assert.Equal(5, doc.ListRoots.Count);
        Assert.All(doc.ListRoots, item => Assert.Empty(item.ContinuationBlocks));
        Assert.Equal(MarkdownBlockKind.Heading, doc.Blocks[1].Kind);
        Assert.Equal(MarkdownBlockKind.CodeFence, doc.Blocks[3].Kind);
        Assert.Equal(MarkdownBlockKind.CodeLine, doc.Blocks[4].Kind);
        Assert.Equal(MarkdownBlockKind.BlockQuote, doc.Blocks[7].Kind);
        Assert.Null(doc.Blocks[7].ListContinuationMarker);
        Assert.Null(doc.Blocks[9].ListContinuationMarker);
        Assert.Equal(source, MarkdownWriter.Write(doc));
    }

    [Fact]
    public void Explicit_then_lazy_then_blank_separated_indented_paragraph_preserve_ownership()
    {
        const string source = "12) Start\n    Explicit\nLazy text\n\n    Another paragraph\n13) Next";
        var doc = MarkdownParser.Parse(source);
        var first = doc.ListRoots[0];
        Assert.True(first.IsLoose);
        Assert.Equal(3, first.ContinuationBlocks.Count);
        Assert.False(first.ContinuationBlocks[0].IsLazyListContinuation);
        Assert.True(first.ContinuationBlocks[1].IsLazyListContinuation);
        Assert.False(first.ContinuationBlocks[2].IsLazyListContinuation);
        Assert.Single(first.SeparatorBlocks);
        Assert.Equal(4, first.ContentBlocks.Count);
        Assert.Equal(source, MarkdownWriter.Write(doc));
    }


    [Fact]
    public void Fence_language_and_literal_code_are_source_aware()
    {
        const string source = "~~~csharp title=Demo\r\n# not a heading\r\n- not a list\r\n~~~\r\n# outside";
        var doc = MarkdownParser.Parse(source);
        Assert.Equal(new[] { MarkdownBlockKind.CodeFence, MarkdownBlockKind.CodeLine,
            MarkdownBlockKind.CodeLine, MarkdownBlockKind.CodeFence, MarkdownBlockKind.Heading },
            doc.Blocks.Select(b => b.Kind));
        Assert.Equal("csharp", doc.Blocks[0].CodeLanguage);
        Assert.Equal("csharp title=Demo", doc.Blocks[0].CodeFenceInfo);
        Assert.Equal("csharp", doc.Blocks[1].CodeLanguage);
        Assert.Equal(doc.Blocks[0].Start, doc.Blocks[2].CodeFenceStart);
        Assert.True(doc.Blocks[3].IsClosingCodeFence);
        Assert.Empty(doc.Blocks[1].Inlines);
        Assert.Equal(source, MarkdownWriter.Write(doc));
    }

    [Fact]
    public void Indented_fences_belong_to_list_and_hide_markdown_inside_code()
    {
        const string source = "- First\r\n  \u0060\u0060\u0060bash\r\n  echo '[red] literal'\r\n  - not a new list\r\n  \u0060\u0060\u0060\r\n  > After code\r\n- Next";
        var doc = MarkdownParser.Parse(source);
        var owner = doc.ListRoots[0];
        Assert.Equal(2, doc.ListRoots.Count);
        Assert.Equal(5, owner.EmbeddedBlocks.Count);
        Assert.Equal(new[] { MarkdownBlockKind.CodeFence, MarkdownBlockKind.CodeLine,
            MarkdownBlockKind.CodeLine, MarkdownBlockKind.CodeFence, MarkdownBlockKind.BlockQuote },
            owner.EmbeddedBlocks.Select(b => b.Kind));
        Assert.All(owner.EmbeddedBlocks, b => Assert.Equal(owner.Block.Start, b.ListContinuationOwnerStart));
        Assert.Equal("bash", doc.Blocks[1].CodeLanguage);
        Assert.Equal("echo '[red] literal'", doc.Blocks[2].Text);
        Assert.True(doc.Blocks[4].IsClosingCodeFence);
        Assert.Equal("After code", doc.Blocks[5].Text);
        Assert.Equal(1, doc.Blocks[5].QuoteDepth);
        Assert.Equal(source, MarkdownWriter.Write(doc));
    }

    [Fact]
    public void Nested_numbered_list_owns_indented_fence_and_resumes_parent()
    {
        const string source = "- Parent\n  12) Child\n      ~~~python\n      print('hello')\n      ~~~\n  Parent again";
        var doc = MarkdownParser.Parse(source);
        var parent = Assert.Single(doc.ListRoots);
        var child = Assert.Single(parent.Children);
        Assert.Equal(3, child.EmbeddedBlocks.Count);
        Assert.Equal("python", child.EmbeddedBlocks[0].CodeLanguage);
        Assert.Equal("print('hello')", child.EmbeddedBlocks[1].Text);
        Assert.Equal("Parent again", Assert.Single(parent.ContinuationBlocks).Text);
        Assert.Equal(source, MarkdownWriter.Write(doc));
    }

    [Fact]
    public void Quotes_are_source_preserving_and_have_inline_offsets()
    {
        const string source = "> **strong** [guide](../en.md)\r\n>> Nested quote\r\n> Last";
        var doc = MarkdownParser.Parse(source);
        Assert.All(doc.Blocks, b => Assert.Equal(MarkdownBlockKind.BlockQuote, b.Kind));
        Assert.Equal(new[] { 1, 2, 1 }, doc.Blocks.Select(b => b.QuoteDepth));
        var link = Assert.Single(doc.Blocks[0].Inlines, n => n.Kind == MarkdownInlineKind.Link);
        Assert.Equal(link.RawText, source.Substring(link.Start, link.Length));
        Assert.Equal(source, MarkdownWriter.Write(doc));
    }

    [Fact]
    public void Unindented_quote_interrupts_list_while_indented_quote_belongs_to_item()
    {
        const string source = "- First\n  > Inside\n> Outside\n- Second";
        var doc = MarkdownParser.Parse(source);
        Assert.Equal(2, doc.ListRoots.Count);
        Assert.Single(doc.ListRoots[0].EmbeddedBlocks);
        Assert.Equal(doc.ListRoots[0].Block.Start, doc.Blocks[1].ListContinuationOwnerStart);
        Assert.Null(doc.Blocks[2].ListContinuationOwnerStart);
        Assert.Equal(MarkdownBlockKind.BlockQuote, doc.Blocks[2].Kind);
        Assert.Equal(source, MarkdownWriter.Write(doc));
    }

    [Fact]
    public void Code_and_quotes_after_blank_separator_do_not_turn_code_into_markdown()
    {
        const string source = "- Item\n\n  \u0060\u0060\u0060json\n  {\"key\":\"# literal\"}\n  \u0060\u0060\u0060\n\n  > Follow-up";
        var doc = MarkdownParser.Parse(source);
        var owner = Assert.Single(doc.ListRoots);
        Assert.True(owner.IsLoose);
        Assert.Equal(2, owner.SeparatorBlocks.Count);
        Assert.Equal(4, owner.EmbeddedBlocks.Count);
        Assert.Equal(MarkdownBlockKind.CodeLine, doc.Blocks[3].Kind);
        Assert.Equal("json", doc.Blocks[2].CodeLanguage);
        Assert.Equal(source, MarkdownWriter.Write(doc));
    }


    [Fact]
    public void Quoted_nested_lists_get_independent_hierarchy_and_keep_exact_offsets()
    {
        const string source = "> - **First** [guide](../docs/en.md)\r\n>   12) Child\r\n>     * Grandchild\r\n>   13) Next\r\n> - Second\r\n";
        var doc = MarkdownParser.Parse(source);
        Assert.Empty(doc.ListRoots);
        Assert.Equal(2, doc.QuoteListRoots.Count);
        Assert.Equal(2, doc.QuoteListRoots[0].Children.Count);
        Assert.Single(doc.QuoteListRoots[0].Children[0].Children);
        Assert.Equal(new[] { 0, 1, 2, 1, 0 },
            doc.Blocks.Select(b => b.ListMarker!.Depth));
        Assert.All(doc.Blocks, b => Assert.Equal(1, b.QuoteDepth));
        Assert.Equal(12, doc.Blocks[1].ListMarker!.Number);
        var link = Assert.Single(doc.Blocks[0].Inlines, n => n.Kind == MarkdownInlineKind.Link);
        Assert.Equal("../docs/en.md", link.Destination);
        Assert.Equal(link.RawText, source.Substring(link.Start, link.Length));
        Assert.Equal(source, MarkdownWriter.Write(doc));
        Assert.All(doc.Blocks, b => Assert.Equal(b.RawText, source.Substring(b.Start, b.Length)));
    }

    [Fact]
    public void Consecutive_quoted_list_paragraphs_keep_the_same_owner()
    {
        const string source = "> 7) Entry\n>    Explicit detail\n> Lazy detail\n> 8) Next";
        var doc = MarkdownParser.Parse(source);
        var first = doc.QuoteListRoots[0];
        Assert.Equal("7)", first.Block.ListMarker!.OriginalMarker);
        Assert.Equal(2, first.ContinuationBlocks.Count);
        Assert.All(first.ContinuationBlocks, b =>
        {
            Assert.Equal(1, b.QuoteDepth);
            Assert.Equal(first.Block.Start, b.ListContinuationOwnerStart);
        });
        Assert.False(first.ContinuationBlocks[0].IsLazyListContinuation);
        Assert.True(first.ContinuationBlocks[1].IsLazyListContinuation);
        Assert.Equal(source, MarkdownWriter.Write(doc));
    }

    [Fact]
    public void Quoted_lists_do_not_join_normal_lists_or_other_quote_levels()
    {
        const string source = "- Outside\n> - First\n>> - Inner level\n> - New first level\n- Outside again\n> - New quoted run";
        var doc = MarkdownParser.Parse(source);
        Assert.Equal(2, doc.ListRoots.Count);
        Assert.Equal(4, doc.QuoteListRoots.Count);
        Assert.All(doc.QuoteListRoots, item => Assert.Empty(item.Children));
        Assert.Equal(new[] { 0, 1, 2, 1, 0, 1 }, doc.Blocks.Select(b => b.QuoteDepth));
        Assert.Equal(source, MarkdownWriter.Write(doc));
    }

    [Fact]
    public void Quote_text_after_list_boundary_stays_a_quote_not_an_unrelated_list_child()
    {
        const string source = "> - Item\n> Still attached\n> # Heading breaks lazy list\n> - New item\n> \n> Outside quote paragraph";
        var doc = MarkdownParser.Parse(source);
        Assert.Equal(2, doc.QuoteListRoots.Count);
        Assert.Single(doc.QuoteListRoots[0].ContinuationBlocks);
        Assert.Equal(MarkdownBlockKind.BlockQuote, doc.Blocks[2].Kind);
        Assert.Equal(MarkdownBlockKind.BlockQuote, doc.Blocks[4].Kind);
        Assert.Equal(MarkdownBlockKind.BlockQuote, doc.Blocks[5].Kind);
        Assert.Null(doc.Blocks[5].ListContinuationOwnerStart);
        Assert.Equal(source, MarkdownWriter.Write(doc));
    }

    [Fact]
    public void Quote_prefixed_fence_is_a_separate_block_and_never_executes_or_parses_code()
    {
        const string source = "> - Item\n> ```bash\n> echo \"literal\"\n> - code, not a list\n> ```\n- Outside";
        var doc = MarkdownParser.Parse(source);
        Assert.Single(doc.QuoteListRoots);
        Assert.Single(doc.ListRoots);
        Assert.Equal(new[] { MarkdownBlockKind.ListItem, MarkdownBlockKind.CodeFence,
            MarkdownBlockKind.CodeLine, MarkdownBlockKind.CodeLine, MarkdownBlockKind.CodeFence,
            MarkdownBlockKind.ListItem }, doc.Blocks.Select(b => b.Kind));
        Assert.Equal("bash", doc.Blocks[1].CodeLanguage);
        Assert.All(doc.Blocks.Skip(1).Take(4), b =>
        {
            Assert.Equal(1, b.QuoteDepth);
            Assert.Null(b.ListContinuationOwnerStart);
            Assert.Equal(doc.Blocks[1].Start, b.CodeFenceStart);
        });
        Assert.True(doc.Blocks[4].IsClosingCodeFence);
        Assert.Empty(doc.Blocks[3].Inlines);
        Assert.Equal(source, MarkdownWriter.Write(doc));
    }


    [Fact]
    public void Quoted_fence_preserves_literal_markers_CRLF_blank_and_nested_quote_levels()
    {
        const string source = ">> ~~~~csharp title=Example\r\n>> using System;\r\n>> \r\n>> # not heading\r\n>> - not a list\r\n>> ``` not another fence\r\n>> ~~~~\r\n>> - Actual list";
        var doc = MarkdownParser.Parse(source);
        Assert.Empty(doc.ListRoots);
        Assert.Single(doc.QuoteListRoots);
        Assert.Equal(new[] { MarkdownBlockKind.CodeFence, MarkdownBlockKind.CodeLine,
            MarkdownBlockKind.CodeLine, MarkdownBlockKind.CodeLine, MarkdownBlockKind.CodeLine,
            MarkdownBlockKind.CodeLine, MarkdownBlockKind.CodeFence, MarkdownBlockKind.ListItem },
            doc.Blocks.Select(b => b.Kind));
        Assert.Equal("csharp", doc.Blocks[0].CodeLanguage);
        Assert.Equal("csharp title=Example", doc.Blocks[0].CodeFenceInfo);
        Assert.Equal("", doc.Blocks[2].Text);
        Assert.All(doc.Blocks, b => Assert.Equal(2, b.QuoteDepth));
        Assert.All(doc.Blocks.Take(7), b =>
        {
            Assert.Equal(doc.Blocks[0].Start, b.CodeFenceStart);
            Assert.Equal(b.RawText, source.Substring(b.Start, b.Length));
        });
        Assert.True(doc.Blocks[6].IsClosingCodeFence);
        Assert.Equal(source, MarkdownWriter.Write(doc));
    }

    [Fact]
    public void Quoted_numbered_list_owns_its_fence_and_preserves_code_indentation()
    {
        const string source = "> - Parent\n>   12) C# example\n>       ```csharp\n>       if (true)\n>       {\n>           Console.WriteLine(\"[red]literal[/]\");\n>       }\n>       ```\n>       After code\n>   Parent detail";
        var doc = MarkdownParser.Parse(source);
        Assert.Empty(doc.ListRoots);
        var parent = Assert.Single(doc.QuoteListRoots);
        var child = Assert.Single(parent.Children);
        Assert.Equal(6, child.EmbeddedBlocks.Count);
        Assert.Equal(new[] { MarkdownBlockKind.CodeFence, MarkdownBlockKind.CodeLine,
            MarkdownBlockKind.CodeLine, MarkdownBlockKind.CodeLine, MarkdownBlockKind.CodeLine,
            MarkdownBlockKind.CodeFence }, doc.Blocks.Skip(2).Take(6).Select(b => b.Kind));
        Assert.Equal("csharp", doc.Blocks[2].CodeLanguage);
        Assert.Equal("    Console.WriteLine(\"[red]literal[/]\");", doc.Blocks[5].Text);
        Assert.All(child.EmbeddedBlocks, b => Assert.Equal(child.Block.Start, b.ListContinuationOwnerStart));
        Assert.Equal("After code", doc.Blocks[8].Text);
        Assert.Equal(child.Block.Start, doc.Blocks[8].ListContinuationOwnerStart);
        Assert.Equal("Parent detail", doc.Blocks[9].Text);
        Assert.Equal(parent.Block.Start, doc.Blocks[9].ListContinuationOwnerStart);
        Assert.Equal(source, MarkdownWriter.Write(doc));
    }

    [Fact]
    public void Quoted_fence_interruption_reclassifies_unquoted_and_differently_nested_lines()
    {
        const string source = "> ```csharp\n> - literal code\nunquoted paragraph\n- outside list\n>> - other quote list";
        var doc = MarkdownParser.Parse(source);
        Assert.Equal(MarkdownBlockKind.CodeFence, doc.Blocks[0].Kind);
        Assert.Equal(MarkdownBlockKind.CodeLine, doc.Blocks[1].Kind);
        Assert.Equal(MarkdownBlockKind.Paragraph, doc.Blocks[2].Kind);
        Assert.Null(doc.Blocks[2].CodeFenceStart);
        Assert.Equal(MarkdownBlockKind.ListItem, doc.Blocks[3].Kind);
        Assert.Equal(1, doc.ListRoots.Count);
        Assert.Equal(1, doc.QuoteListRoots.Count);
        Assert.Equal(2, doc.QuoteListRoots[0].Block.QuoteDepth);
        Assert.Equal(source, MarkdownWriter.Write(doc));

        var differentDepth = MarkdownParser.Parse("> ```csharp\n>> not code\n>> - actual quoted item");
        Assert.Equal(MarkdownBlockKind.BlockQuote, differentDepth.Blocks[1].Kind);
        Assert.Equal(MarkdownBlockKind.ListItem, differentDepth.Blocks[2].Kind);
        Assert.Null(differentDepth.Blocks[1].CodeFenceStart);
        Assert.Single(differentDepth.QuoteListRoots);
    }

    [Fact]
    public void Quoted_tilde_fence_only_closes_with_matching_marker_and_length()
    {
        const string source = "> ~~~~js\n> ~~~\n> ````\n> ~~~~\n> - next";
        var doc = MarkdownParser.Parse(source);
        Assert.Equal(MarkdownBlockKind.CodeLine, doc.Blocks[1].Kind);
        Assert.Equal(MarkdownBlockKind.CodeLine, doc.Blocks[2].Kind);
        Assert.True(doc.Blocks[3].IsClosingCodeFence);
        Assert.Equal(MarkdownBlockKind.ListItem, doc.Blocks[4].Kind);
        Assert.Equal(source, MarkdownWriter.Write(doc));
    }

    [Fact]
    public void Quoted_list_attaches_multiple_blank_markers_only_when_indented_paragraph_follows()
    {
        const string source = "> 12) First\r\n> \r\n>\r\n>     Second **bold** [guide](../en.md)\r\n> 13) Next";
        var doc = MarkdownParser.Parse(source);
        Assert.Empty(doc.ListRoots);
        Assert.Equal(2, doc.QuoteListRoots.Count);
        var owner = doc.QuoteListRoots[0];
        Assert.True(owner.IsLoose);
        Assert.Equal(2, owner.SeparatorBlocks.Count);
        Assert.Equal(new[] { MarkdownBlockKind.BlockQuote, MarkdownBlockKind.BlockQuote },
            owner.SeparatorBlocks.Select(b => b.Kind));
        Assert.Equal(new[] { doc.Blocks[1].Start, doc.Blocks[2].Start, doc.Blocks[3].Start },
            owner.ContentBlocks.Select(b => b.Start));
        Assert.All(owner.SeparatorBlocks, b =>
        {
            Assert.Equal(1, b.QuoteDepth);
            Assert.Equal(owner.Block.Start, b.ListBlankOwnerStart);
        });
        Assert.Equal(owner.Block.Start, doc.Blocks[3].ListContinuationOwnerStart);
        Assert.Equal("Second **bold** [guide](../en.md)", doc.Blocks[3].Text);
        var link = Assert.Single(doc.Blocks[3].Inlines,
            n => n.Kind == MarkdownInlineKind.Link);
        Assert.Equal(link.RawText, source.Substring(link.Start, link.Length));
        Assert.All(doc.Blocks, b =>
            Assert.Equal(b.RawText, source.Substring(b.Start, b.Length)));
        Assert.Equal(source, MarkdownWriter.Write(doc));
    }

    [Fact]
    public void Quoted_blank_after_nested_numbered_child_can_belong_to_parent()
    {
        const string source = "> - Parent\n>   12) Child\n> \n>   Back to parent";
        var doc = MarkdownParser.Parse(source);
        var parent = Assert.Single(doc.QuoteListRoots);
        var child = Assert.Single(parent.Children);
        Assert.True(parent.IsLoose);
        Assert.False(child.IsLoose);
        Assert.Single(parent.SeparatorBlocks);
        Assert.Equal(parent.Block.Start, doc.Blocks[2].ListBlankOwnerStart);
        Assert.Equal(parent.Block.Start, doc.Blocks[3].ListContinuationOwnerStart);
        Assert.Equal("Back to parent", doc.Blocks[3].Text);
        Assert.Equal(source, MarkdownWriter.Write(doc));
    }

    [Fact]
    public void Quote_blank_cannot_make_an_unindented_paragraph_or_new_item_a_loose_continuation()
    {
        const string paragraph = "> - First\n> \n> Separate paragraph\n>     Not attached";
        var doc = MarkdownParser.Parse(paragraph);
        var item = Assert.Single(doc.QuoteListRoots);
        Assert.False(item.IsLoose);
        Assert.Null(doc.Blocks[1].ListBlankOwnerStart);
        Assert.Equal(MarkdownBlockKind.BlockQuote, doc.Blocks[2].Kind);
        Assert.Null(doc.Blocks[2].ListContinuationOwnerStart);
        Assert.Equal(MarkdownBlockKind.BlockQuote, doc.Blocks[3].Kind);
        Assert.Equal(paragraph, MarkdownWriter.Write(doc));

        const string marker = "> - One\n> \n> - Two";
        var withNewMarker = MarkdownParser.Parse(marker);
        Assert.Equal(2, withNewMarker.QuoteListRoots.Count);
        Assert.All(withNewMarker.QuoteListRoots, node => Assert.False(node.IsLoose));
        Assert.Null(withNewMarker.Blocks[1].ListBlankOwnerStart);
        Assert.Equal(marker, MarkdownWriter.Write(withNewMarker));
    }

    [Theory]
    [InlineData("> - Item\n> \n>>   Different quote depth")]
    [InlineData("> - Item\n> \n- Outside list")]
    [InlineData("> - Item\n> \n\n>   Restart without attachment")]
    public void Quoted_blank_does_not_cross_quote_depth_or_unquoted_boundary(string source)
    {
        var doc = MarkdownParser.Parse(source);
        var owner = Assert.Single(doc.QuoteListRoots);
        Assert.False(owner.IsLoose);
        Assert.Null(doc.Blocks[1].ListBlankOwnerStart);
        Assert.Equal(source, MarkdownWriter.Write(doc));
    }

    [Fact]
    public void Quoted_blank_before_indented_fence_is_owned_but_code_blanks_are_literal()
    {
        const string source = "> - Item\n> \n>   ~~~bash\n>   echo '[red]literal[/]'\n> \n>   ~~~\n>   After";
        var doc = MarkdownParser.Parse(source);
        var owner = Assert.Single(doc.QuoteListRoots);
        Assert.True(owner.IsLoose);
        Assert.Single(owner.SeparatorBlocks);
        Assert.Equal(owner.Block.Start, doc.Blocks[1].ListBlankOwnerStart);
        Assert.Equal(MarkdownBlockKind.CodeFence, doc.Blocks[2].Kind);
        Assert.Equal("bash", doc.Blocks[2].CodeLanguage);
        Assert.Equal(MarkdownBlockKind.CodeLine, doc.Blocks[4].Kind);
        Assert.Null(doc.Blocks[4].ListBlankOwnerStart);
        Assert.Equal("", doc.Blocks[4].Text);
        Assert.True(doc.Blocks[5].IsClosingCodeFence);
        Assert.Equal(owner.Block.Start, doc.Blocks[6].ListContinuationOwnerStart);
        Assert.Equal(source, MarkdownWriter.Write(doc));
    }


    [Fact]
    public void Quoted_list_inside_ordinary_list_preserves_independent_owners_and_offsets()
    {
        const string source = "- Install\r\n  > - **Caution** [guide](../en.md)\r\n  >   12) Risk\r\n  >     Note\r\n  > - Second risk\r\n  Continue install\r\n- Finish";
        var doc = MarkdownParser.Parse(source);
        Assert.Equal(2, doc.ListRoots.Count);
        Assert.Equal(2, doc.QuoteListRoots.Count);
        var outer = doc.ListRoots[0];
        var quoted = doc.QuoteListRoots[0];
        Assert.Single(quoted.Children);
        Assert.Equal(4, outer.EmbeddedBlocks.Count);
        Assert.Equal(new[] { 0, 1, 0 },
            doc.Blocks.Skip(1).Take(4).Where(b => b.ListMarker is not null)
                .Select(b => b.ListMarker!.Depth));
        Assert.All(doc.Blocks.Skip(1).Take(4), b =>
        {
            Assert.Equal(outer.Block.Start, b.QuoteContainerListOwnerStart);
            Assert.Equal(1, b.QuoteDepth);
            Assert.Equal(b.RawText, source.Substring(b.Start, b.Length));
        });
        Assert.Equal(quoted.Children[0].Block.Start, doc.Blocks[3].ListContinuationOwnerStart);
        Assert.Equal(outer.Block.Start, doc.Blocks[5].ListContinuationOwnerStart);
        var link = Assert.Single(doc.Blocks[1].Inlines, n => n.Kind == MarkdownInlineKind.Link);
        Assert.Equal(link.RawText, source.Substring(link.Start, link.Length));
        Assert.Equal(source, MarkdownWriter.Write(doc));
    }

    [Fact]
    public void Quoted_list_runs_do_not_merge_across_outer_item_boundaries()
    {
        const string source = "- First\n  > - Child A\n- Second\n  > - Child B\n> - Standalone";
        var doc = MarkdownParser.Parse(source);
        Assert.Equal(2, doc.ListRoots.Count);
        Assert.Equal(3, doc.QuoteListRoots.Count);
        Assert.All(doc.QuoteListRoots, node => Assert.Empty(node.Children));
        Assert.Equal(doc.ListRoots[0].Block.Start, doc.QuoteListRoots[0].Block.QuoteContainerListOwnerStart);
        Assert.Equal(doc.ListRoots[1].Block.Start, doc.QuoteListRoots[1].Block.QuoteContainerListOwnerStart);
        Assert.Null(doc.QuoteListRoots[2].Block.QuoteContainerListOwnerStart);
        Assert.Equal(source, MarkdownWriter.Write(doc));
    }

    [Fact]
    public void Nested_ordinary_numbered_item_owns_its_quoted_sublist()
    {
        const string source = "- Parent\n  12) Nested\n      > - **Note**\n      >   Extra detail\n  Parent resumes";
        var doc = MarkdownParser.Parse(source);
        var root = Assert.Single(doc.ListRoots);
        var child = Assert.Single(root.Children);
        var quoted = Assert.Single(doc.QuoteListRoots);
        Assert.Equal(child.Block.Start, quoted.Block.QuoteContainerListOwnerStart);
        Assert.Equal(child.Block.Start, doc.Blocks[3].QuoteContainerListOwnerStart);
        Assert.Equal(quoted.Block.Start, doc.Blocks[3].ListContinuationOwnerStart);
        Assert.Equal(root.Block.Start, doc.Blocks[4].ListContinuationOwnerStart);
        Assert.Equal(source, MarkdownWriter.Write(doc));
    }

    [Fact]
    public void Quoted_fence_in_ordinary_list_quoted_sublist_protects_literal_lines()
    {
        const string source = "- Step\n  > - Example\n  >   ~~~csharp\n  >   # not heading\n  >   - not a list\n  >   Console.WriteLine(\"[red]literal[/]\");\n  >   ~~~\n  Continue";
        var doc = MarkdownParser.Parse(source);
        var root = Assert.Single(doc.ListRoots);
        var quoted = Assert.Single(doc.QuoteListRoots);
        Assert.Equal(5, quoted.EmbeddedBlocks.Count);
        Assert.Equal(new[] { MarkdownBlockKind.CodeFence, MarkdownBlockKind.CodeLine,
            MarkdownBlockKind.CodeLine, MarkdownBlockKind.CodeLine, MarkdownBlockKind.CodeFence },
            doc.Blocks.Skip(2).Take(5).Select(b => b.Kind));
        Assert.All(doc.Blocks.Skip(2).Take(5), b =>
        {
            Assert.Equal(root.Block.Start, b.QuoteContainerListOwnerStart);
            Assert.Equal(quoted.Block.Start, b.ListContinuationOwnerStart);
        });
        Assert.Empty(doc.Blocks[3].Inlines);
        Assert.True(doc.Blocks[6].IsClosingCodeFence);
        Assert.Equal(root.Block.Start, doc.Blocks[7].ListContinuationOwnerStart);
        Assert.Equal(source, MarkdownWriter.Write(doc));
    }

    [Fact]
    public void Quote_blank_in_outer_list_is_owned_by_inner_quoted_item()
    {
        const string source = "- Step\n  > 12) Warning\n  >\n  >     More **detail**\n- Next";
        var doc = MarkdownParser.Parse(source);
        var outer = doc.ListRoots[0];
        var quoted = Assert.Single(doc.QuoteListRoots);
        Assert.True(quoted.IsLoose);
        Assert.Equal(quoted.Block.Start, doc.Blocks[2].ListBlankOwnerStart);
        Assert.Equal(quoted.Block.Start, doc.Blocks[3].ListContinuationOwnerStart);
        Assert.Equal(outer.Block.Start, doc.Blocks[3].QuoteContainerListOwnerStart);
        Assert.Equal(source, MarkdownWriter.Write(doc));
    }

    [Fact]
    public void Existing_plain_indented_quote_keeps_ordinary_owner()
    {
        const string source = "- Step\n  > Plain **note**\n  Continue";
        var doc = MarkdownParser.Parse(source);
        var outer = Assert.Single(doc.ListRoots);
        Assert.Empty(doc.QuoteListRoots);
        Assert.Equal(MarkdownBlockKind.BlockQuote, doc.Blocks[1].Kind);
        Assert.Equal(outer.Block.Start, doc.Blocks[1].ListContinuationOwnerStart);
        Assert.Equal(outer.Block.Start, doc.Blocks[1].QuoteContainerListOwnerStart);
        Assert.Equal(source, MarkdownWriter.Write(doc));
    }

    [Fact]
    public void Null_arguments_fail_explicitly()
    {
        Assert.Throws<ArgumentNullException>(() => MarkdownParser.Parse(null!));
        Assert.Throws<ArgumentNullException>(() => MarkdownWriter.Write(null!));
    }
}
