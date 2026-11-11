using Afrowave.Toolbox.Blackboard.Tools.MdView;
using Xunit;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView.Tests;

/// <summary>Smart terminal wrapping and code-source integrity regression coverage.</summary>
public sealed class MdViewTerminalWordWrapTests
{
    [Fact]
    public void ProseWrapPrefersSentenceAndCommaBoundaries()
    {
        const string source =
            "A complete sentence. Another short clause, followed by more words here.";
        var rows = MdViewTerminalWordWrap.Wrap(source, 42);

        Assert.True(rows.Count >= 2);
        Assert.EndsWith(".", rows[0]);
        Assert.Equal(source,
            string.Join(" ", rows)); // Soft wrapping preserves word order.
        Assert.All(rows, row =>
            Assert.True(MdViewTerminalWordWrap.MeasureColumns(row) <= 42));
    }

    [Fact]
    public void LongWordIsSplitOnlyWhenNoWordBoundaryFits()
    {
        var word = new string('x', 49);
        var rows = MdViewTerminalWordWrap.Wrap(word, 12);

        Assert.Equal(5, rows.Count);
        Assert.Equal(word, string.Concat(rows));
        Assert.All(rows, row => Assert.True(row.Length <= 12));
    }

    [Fact]
    public void CodeIsNeverSoftWrappedAndShowsRightArrowForHiddenColumns()
    {
        const string source = "    return some_long_function(12345, value);";
        var rows = MdViewTerminalWordWrap.Wrap(source, 22, code: true);

        Assert.Single(rows);
        Assert.EndsWith(MdViewTerminalWordWrap.CodeOverflowMarker, rows[0]);
        Assert.StartsWith("    return ", rows[0]);
        Assert.True(MdViewTerminalWordWrap.MeasureColumns(rows[0]) <= 22);
        Assert.Equal(source,
            MdViewTerminalWordWrap.Wrap(source, 80, code: true)[0]);
    }

    [Fact]
    public void EmojiAndCjkDoNotGetSplitAcrossSoftOrCodeRows()
    {
        const string source = "Czech čeština 😀 日本語 test another sentence.";
        var rows = MdViewTerminalWordWrap.Wrap(source, 10);

        Assert.Contains("😀", string.Join("", rows));
        Assert.Contains("日本語", string.Join("", rows));
        Assert.DoesNotContain(rows,
            row => row.Length > 0 && char.IsHighSurrogate(row[^1]));
        Assert.DoesNotContain(rows,
            row => row.Length > 0 && char.IsLowSurrogate(row[0]));
        Assert.All(rows, row =>
            Assert.True(MdViewTerminalWordWrap.MeasureColumns(row) <= 10));

        var code = MdViewTerminalWordWrap.Wrap("    😀😀😀😀😀😀", 11, code: true);
        Assert.Single(code);
        Assert.EndsWith("→", code[0]);
        Assert.False(char.IsHighSurrogate(code[0][^2]));
    }

    [Fact]
    public void EmptySourceAndInvalidWidthsAreHandled()
    {
        Assert.Equal("", Assert.Single(MdViewTerminalWordWrap.Wrap("", 40)));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            MdViewTerminalWordWrap.Wrap("hello", 7));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            MdViewTerminalWordWrap.Wrap("hello", 4097));
        Assert.Throws<ArgumentNullException>(() =>
            MdViewTerminalWordWrap.Wrap(null!, 40));
    }
}
