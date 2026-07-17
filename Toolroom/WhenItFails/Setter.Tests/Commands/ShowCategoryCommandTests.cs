using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

public sealed class ShowCategoryCommandTests
{
    [Fact]
    public void TryParseOptions_WithoutOptionalSwitch_ReturnsRichOutput()
    {
        bool result = ShowCategoryCommand.TryParseOptions(
            ["show-category", ".", "NETWORK"],
            out bool usePlainOutput,
            out bool useJsonOutput);

        Assert.True(result);
        Assert.False(usePlainOutput);
        Assert.False(useJsonOutput);
    }

    [Fact]
    public void TryParseOptions_WithPlainSwitch_ReturnsPlainOutput()
    {
        bool result = ShowCategoryCommand.TryParseOptions(
            ["show-category", ".", "NETWORK", "--plain"],
            out bool usePlainOutput,
            out bool useJsonOutput);

        Assert.True(result);
        Assert.True(usePlainOutput);
        Assert.False(useJsonOutput);
    }

    [Fact]
    public void TryParseOptions_WithJsonSwitch_ReturnsJsonOutput()
    {
        bool result = ShowCategoryCommand.TryParseOptions(
            ["show-category", ".", "NETWORK", "--JSON"],
            out bool usePlainOutput,
            out bool useJsonOutput);

        Assert.True(result);
        Assert.False(usePlainOutput);
        Assert.True(useJsonOutput);
    }

    [Fact]
    public void TryParseOptions_WithUnknownArgument_ReturnsFalse()
    {
        bool result = ShowCategoryCommand.TryParseOptions(
            ["show-category", ".", "NETWORK", "--unknown"],
            out _,
            out _);

        Assert.False(result);
    }

    [Theory]
    [InlineData("--plain", "--plain")]
    [InlineData("--json", "--json")]
    [InlineData("--plain", "--json")]
    [InlineData("--json", "--plain")]
    public void TryParseOptions_WithConflictingOutputSwitches_ReturnsFalse(
        string firstSwitch,
        string secondSwitch)
    {
        bool result = ShowCategoryCommand.TryParseOptions(
            ["show-category", ".", "NETWORK", firstSwitch, secondSwitch],
            out _,
            out _);

        Assert.False(result);
    }

    [Fact]
    public void FindCategory_ByNameDisplayNameOrAlias_ReturnsCategory()
    {
        WhenItFailsWorkspaceSummary summary = new();
        ErrorCategoryDefinition category = new()
        {
            Name = "EXTERNAL_SERVICE",
            DisplayName = "External service",
            Aliases = ["REMOTE_SERVICE"]
        };
        summary.CategoryCatalog.Categories.Add(category);

        Assert.Same(category, ShowCategoryCommand.FindCategory(summary, "external_service"));
        Assert.Same(category, ShowCategoryCommand.FindCategory(summary, "External service"));
        Assert.Same(category, ShowCategoryCommand.FindCategory(summary, "remote_service"));
    }

    [Fact]
    public void FindCategory_WithUnknownName_ReturnsNull()
    {
        WhenItFailsWorkspaceSummary summary = new();

        Assert.Null(ShowCategoryCommand.FindCategory(summary, "UNKNOWN"));
    }
}
