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
    public void Null_arguments_fail_explicitly()
    {
        Assert.Throws<ArgumentNullException>(() => MarkdownParser.Parse(null!));
        Assert.Throws<ArgumentNullException>(() => MarkdownWriter.Write(null!));
    }
}
