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

        var strong = Assert.Single(block.Inlines.Where(n => n.Kind == MarkdownInlineKind.Strong));
        Assert.Equal("the guide", strong.Text);
        var link = Assert.Single(block.Inlines.Where(n => n.Kind == MarkdownInlineKind.Link));
        Assert.Equal("our *docs*", link.Text);
        Assert.Equal("../en/spec_(draft).md", link.Destination);
        Assert.Equal("docs", Assert.Single(link.Children.Where(n => n.Kind == MarkdownInlineKind.Emphasis)).Text);
        var code = Assert.Single(block.Inlines.Where(n => n.Kind == MarkdownInlineKind.Code));
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
        var strong = Assert.Single(nodes.Where(n => n.Kind == MarkdownInlineKind.Strong));
        Assert.Equal("italic", Assert.Single(strong.Children.Where(n => n.Kind == MarkdownInlineKind.Emphasis)).Text);
        Assert.Contains(nodes, n => n.Kind == MarkdownInlineKind.Emphasis && n.Text == "more");
        foreach (var n in nodes)
            CheckRange(source, n);
    }

    [Fact]
    public void Escaped_markers_and_escaped_brackets_are_not_misinterpreted()
    {
        const string source = @"\*literal\* then [a \] label](some_(folder).md) and file_name.md";
        var nodes = MarkdownInlineParser.Parse(source);
        var link = Assert.Single(nodes.Where(n => n.Kind == MarkdownInlineKind.Link));
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
        var code = Assert.Single(MarkdownInlineParser.Parse(source).Where(n => n.Kind == MarkdownInlineKind.Code));
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

    private static void CheckRange(string source, MarkdownInlineNode node)
    {
        Assert.Equal(node.RawText, source.Substring(node.Start, node.Length));
        foreach (var child in node.Children)
            CheckRange(source, child);
    }
}
