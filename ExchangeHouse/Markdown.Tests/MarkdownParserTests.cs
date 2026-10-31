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
        const string markdown = "- first\n  attached\n\n  not attached\n- second\nplain\n  also outside";
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
    public void Null_arguments_fail_explicitly()
    {
        Assert.Throws<ArgumentNullException>(() => MarkdownParser.Parse(null!));
        Assert.Throws<ArgumentNullException>(() => MarkdownWriter.Write(null!));
    }
}
