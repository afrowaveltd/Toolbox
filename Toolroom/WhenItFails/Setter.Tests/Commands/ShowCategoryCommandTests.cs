using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

public sealed class ShowCategoryCommandTests
{
    [Fact]
    public void TryParseOptions_WithoutOptionalSwitch_ReturnsTrueAndPlainFalse()
    {
        bool result = ShowCategoryCommand.TryParseOptions(
            ["show-category", ".", "NETWORK"],
            out bool usePlainOutput);

        Assert.True(result);
        Assert.False(usePlainOutput);
    }

    [Fact]
    public void TryParseOptions_WithPlainSwitch_ReturnsTrueAndPlainTrue()
    {
        bool result = ShowCategoryCommand.TryParseOptions(
            ["show-category", ".", "NETWORK", "--plain"],
            out bool usePlainOutput);

        Assert.True(result);
        Assert.True(usePlainOutput);
    }

    [Fact]
    public void TryParseOptions_WithUnknownArgument_ReturnsFalse()
    {
        bool result = ShowCategoryCommand.TryParseOptions(
            ["show-category", ".", "NETWORK", "--unknown"],
            out _);

        Assert.False(result);
    }

    [Fact]
    public void TryParseOptions_WithDuplicatePlainSwitch_ReturnsFalse()
    {
        bool result = ShowCategoryCommand.TryParseOptions(
            ["show-category", ".", "NETWORK", "--plain", "--plain"],
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
