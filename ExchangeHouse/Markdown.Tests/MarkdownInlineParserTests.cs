using Xunit;

namespace Afrowave.Toolbox.ExchangeHouse.Markdown.Tests;

public sealed class MarkdownInlineParserTests
{
    [Fact]
    public void Inline_nodes_preserve_absolute_offsets_and_original_link_destination()
    {
        const string source = "# Read **the guide** at [our *docs*](../en/spec_(draft).md) or `support@example.com`.\r\n";
        var document = MarkdownParser.Parse(source);
        var block = Assert.Single(document.Blocks);
        Assert.Equal(MarkdownBlockKind.Heading, block.Kind);
        Assert.Equal(source, MarkdownWriter.Write(document));
        Assert.Equal(1, block.HeadingLevel);

        var strong = Assert.Single(block.Inlines, n => n.Kind == MarkdownInlineKind.Strong);
        Assert.Equal("the guide", strong.Text);
        var link = Assert.Single(block.Inlines, n => n.Kind == MarkdownInlineKind.Link);
        Assert.Equal("our *docs*", link.Text);
        Assert.Equal("../en/spec_(draft).md", link.Destination);
        Assert.Equal("docs", Assert.Single(link.Children, n => n.Kind == MarkdownInlineKind.Emphasis).Text);
        var code = Assert.Single(block.Inlines, n => n.Kind == MarkdownInlineKind.Code);
        Assert.Equal("support@example.com", code.Text);

        foreach (var node in block.Inlines)
            CheckRange(source, node);
    }

    [Fact]
    public void Nested_emphasis_in_strong_is_tokenized_and_reconstructed()
    {
        const string source = "See **bold with *italic* here** and _more_.";
        var nodes = MarkdownInlineParser.Parse(source);
        Assert.Equal(source, string.Concat(nodes.Select(n => n.RawText)));
        var strong = Assert.Single(nodes, n => n.Kind == MarkdownInlineKind.Strong);
        Assert.Equal("italic", Assert.Single(strong.Children, n => n.Kind == MarkdownInlineKind.Emphasis).Text);
        Assert.Contains(nodes, n => n.Kind == MarkdownInlineKind.Emphasis && n.Text == "more");
        foreach (var n in nodes)
            CheckRange(source, n);
    }

    [Fact]
    public void Escaped_markers_and_escaped_brackets_are_not_misinterpreted()
    {
        const string source = @"\*literal\* then [a \] label](some_(folder).md) and file_name.md";
        var nodes = MarkdownInlineParser.Parse(source);
        var link = Assert.Single(nodes, n => n.Kind == MarkdownInlineKind.Link);
        Assert.Equal(@"a \] label", link.Text);
        Assert.Equal("some_(folder).md", link.Destination);
        Assert.DoesNotContain(nodes, n => n.Kind == MarkdownInlineKind.Emphasis);
        Assert.Equal(source, string.Concat(nodes.Select(n => n.RawText)));
    }

    [Fact]
    public void Malformed_syntax_is_left_as_literal_text()
    {
        const string source = "Go [broken](missing and **unclosed";
        var node = Assert.Single(MarkdownInlineParser.Parse(source));
        Assert.Equal(MarkdownInlineKind.Text, node.Kind);
        Assert.Equal(source, node.RawText);
    }

    [Fact]
    public void Code_spans_do_not_parse_inner_markup_and_support_multiple_backticks()
    {
        const string source = "Before ``a *b* [x](y)`` after";
        var code = Assert.Single(MarkdownInlineParser.Parse(source), n => n.Kind == MarkdownInlineKind.Code);
        Assert.Equal("a *b* [x](y)", code.Text);
        Assert.Empty(code.Children);
    }

    [Fact]
    public void Fenced_code_lines_do_not_receive_inline_tokens()
    {
        var blocks = MarkdownParser.Parse("```md\n**not bold** [label](url)\n```\n").Blocks;
        Assert.All(blocks, b => Assert.Empty(b.Inlines));
    }

    [Fact]
    public void List_item_nodes_use_absolute_document_offsets()
    {
        const string source = "intro\r\n- `command` and **bold**\n";
        var block = MarkdownParser.Parse(source).Blocks[1];
        Assert.Equal(MarkdownBlockKind.ListItem, block.Kind);
        Assert.Equal("`command` and **bold**", block.Text);
        foreach (var node in block.Inlines)
            CheckRange(source, node);
    }

    [Fact]
    public void Arbitrary_source_offset_is_supported()
    {
        var node = Assert.Single(MarkdownInlineParser.Parse("**hi**", 42));
        Assert.Equal(42, node.Start);
        Assert.Equal(6, node.Length);
        Assert.Equal(MarkdownInlineKind.Strong, node.Kind);
        Assert.Throws<ArgumentOutOfRangeException>(() => MarkdownInlineParser.Parse("x", -1));
        Assert.Throws<ArgumentNullException>(() => MarkdownInlineParser.Parse(null!));
    }


    [Fact]
    public void Image_alt_destination_and_title_have_separate_source_preserving_spans()
    {
        const string source = "![Our *logo*](../assets/logo_(v2).png \"Brand image\")";
        var image = Assert.Single(MarkdownInlineParser.Parse(source), n => n.Kind == MarkdownInlineKind.Image);
        Assert.Equal("Our *logo*", image.Text);
        Assert.Equal("../assets/logo_(v2).png", image.Destination);
        Assert.Equal("Brand image", image.Title);
        Assert.Equal("logo", Assert.Single(image.Children, n => n.Kind == MarkdownInlineKind.Emphasis).Text);
        Assert.Equal(image.Destination, source.Substring(image.DestinationStart!.Value, image.DestinationLength));
        CheckRange(source, image);
    }

    [Theory]
    [InlineData("[Guide](<../docs/user guide.md> \"Help\")", "../docs/user guide.md", "Help")]
    [InlineData("[Guide](../docs/setup.md 'Install')", "../docs/setup.md", "Install")]
    [InlineData("[Guide](../docs/setup.md (Install))", "../docs/setup.md", "Install")]
    [InlineData("[Guide](../docs/setup.md)", "../docs/setup.md", null)]
    [InlineData("[Empty]()", "", null)]
    [InlineData("[Nested](foo_(bar).md)", "foo_(bar).md", null)]
    [InlineData("[Escaped](foo\\)bar.md)", "foo\\)bar.md", null)]
    public void Inline_links_separate_unmodified_target_from_optional_title(
        string source, string destination, string? title)
    {
        var link = Assert.Single(MarkdownInlineParser.Parse(source), n => n.Kind == MarkdownInlineKind.Link);
        Assert.Equal(destination, link.Destination);
        Assert.Equal(title, link.Title);
        Assert.Equal(destination, source.Substring(link.DestinationStart!.Value, link.DestinationLength));
        Assert.Equal(source, string.Concat(MarkdownInlineParser.Parse(source).Select(n => n.RawText)));
    }

    [Fact]
    public void Link_with_nested_image_preserves_both_destinations()
    {
        const string source = "[![Logo](img/logo.png)](../index.md)";
        var link = Assert.Single(MarkdownInlineParser.Parse(source));
        Assert.Equal(MarkdownInlineKind.Link, link.Kind);
        Assert.Equal("../index.md", link.Destination);
        var image = Assert.Single(link.Children);
        Assert.Equal(MarkdownInlineKind.Image, image.Kind);
        Assert.Equal("img/logo.png", image.Destination);
        CheckRange(source, link);
    }

    [Theory]
    [InlineData("![bad](image.png \"unclosed)")]
    [InlineData("[bad](<unterminated)")]
    [InlineData("[bad](file.md \"title\" extra)")]
    [InlineData("![bad](file(name.png)")]
    public void Malformed_references_do_not_become_links_or_images(string source)
    {
        var nodes = MarkdownInlineParser.Parse(source);
        Assert.DoesNotContain(nodes, n => n.Kind is MarkdownInlineKind.Link or MarkdownInlineKind.Image);
        Assert.Equal(source, string.Concat(nodes.Select(n => n.RawText)));
    }

    [Fact]
    public void Image_offsets_survive_whole_document_parsing_and_lossless_writing()
    {
        const string source = "# Illustration\r\n![Logo](../assets/logo.png)\r\n";
        var doc = MarkdownParser.Parse(source);
        var image = Assert.Single(doc.Blocks[1].Inlines, n => n.Kind == MarkdownInlineKind.Image);
        Assert.Equal("../assets/logo.png", source.Substring(image.DestinationStart!.Value, image.DestinationLength));
        Assert.Equal(source, MarkdownWriter.Write(doc));
    }

    private static void CheckRange(string source, MarkdownInlineNode node)
    {
        Assert.Equal(node.RawText, source.Substring(node.Start, node.Length));
        foreach (var child in node.Children)
            CheckRange(source, child);
    }
}
