using Afrowave.Toolbox.Blackboard.Tools.MdView;
using Spectre.Console;
using Spectre.Console.Testing;
using Xunit;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView.Tests;

/// <summary>
/// Native Spectre render-segment height measurement with the active
/// console width, independent of Markdown source-line estimates.
/// </summary>
public sealed class MdViewSpectreTableHeightTests
{
    [Fact]
    public void MeasuresRoundedBorderAndHeaderWithoutWritingToConsole()
    {
        var console = Console(width: 80);
        var table = Table(width: 78, "Short cell");

        var rows = MdViewSpectreTableHeight.CountRows(table, console);

        Assert.True(rows >= 5);
        Assert.Equal(string.Empty, console.Output);
        console.Write(table);
        Assert.Contains("╭", console.Output);
        Assert.Contains("Short cell", console.Output);
    }

    [Fact]
    public void LongCellsConsumeMoreRowsOnNarrowViewport()
    {
        var text = string.Join(" ", Enumerable.Repeat("long description", 16));
        var narrow = Console(width: 34);
        var wide = Console(width: 110);
        var narrowTable = Table(32, text);
        var wideTable = Table(108, text);

        var narrowRows = MdViewSpectreTableHeight.CountRows(narrowTable, narrow);
        var wideRows = MdViewSpectreTableHeight.CountRows(wideTable, wide);

        Assert.True(narrowRows > wideRows,
            $"Narrow: {narrowRows}, wide: {wideRows}");
        Assert.Equal(string.Empty, narrow.Output);
        Assert.Equal(string.Empty, wide.Output);
    }

    [Fact]
    public void EarlyStopLimitsMeasurementWithoutWritingAnyWidget()
    {
        var console = Console();
        var table = Table(78, "Value");
        for (var i = 0; i < 20; i++)
            table.AddRow(new Text("Row " + i), new Text("Value " + i));

        var measured = MdViewSpectreTableHeight.CountRows(
            table, console, maximumRows: 5);

        Assert.Equal(6, measured);
        Assert.Equal(string.Empty, console.Output);
    }

    [Fact]
    public void InvalidMeasurementBudgetsAndNullInputsFailPredictably()
    {
        var console = Console();
        var table = Table(78, "Value");

        Assert.Throws<ArgumentNullException>(() =>
            MdViewSpectreTableHeight.CountRows(null!, console));
        Assert.Throws<ArgumentNullException>(() =>
            MdViewSpectreTableHeight.CountRows(table, null!));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            MdViewSpectreTableHeight.CountRows(table, console, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            MdViewSpectreTableHeight.CountRows(
                table, console, MdViewSpectreTableHeight.MaximumMeasuredRows + 1));
    }

    private static TestConsole Console(int width = 80)
    {
        var console = new TestConsole();
        console.Profile.Width = width;
        console.Profile.Height = 35;
        console.Profile.Capabilities.Unicode = true;
        console.Profile.Capabilities.Interactive = true;
        return console;
    }

    private static Table Table(int width, string content)
    {
        var table = new Table().RoundedBorder().Width(width);
        table.AddColumn("Field");
        table.AddColumn("Value");
        table.AddRow(new Text("Name"), new Text(content));
        return table;
    }
}
