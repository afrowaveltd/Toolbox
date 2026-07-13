using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

public sealed class ShowCodeGroupCommandTests
{
    [Fact]
    public void TryParseOptions_WithoutOptionalSwitch_ReturnsTrueAndPlainFalse()
    {
        bool result = ShowCodeGroupCommand.TryParseOptions(
            ["show-code-group", ".", "NETWORK"],
            out bool usePlainOutput);

        Assert.True(result);
        Assert.False(usePlainOutput);
    }

    [Fact]
    public void TryParseOptions_WithPlainSwitch_ReturnsTrueAndPlainTrue()
    {
        bool result = ShowCodeGroupCommand.TryParseOptions(
            ["show-code-group", ".", "NETWORK", "--plain"],
            out bool usePlainOutput);

        Assert.True(result);
        Assert.True(usePlainOutput);
    }

    [Fact]
    public void TryParseOptions_WithCaseInsensitivePlainSwitch_ReturnsTrue()
    {
        bool result = ShowCodeGroupCommand.TryParseOptions(
            ["show-code-group", ".", "NETWORK", "--PLAIN"],
            out bool usePlainOutput);

        Assert.True(result);
        Assert.True(usePlainOutput);
    }

    [Fact]
    public void TryParseOptions_WithUnknownArgument_ReturnsFalse()
    {
        bool result = ShowCodeGroupCommand.TryParseOptions(
            ["show-code-group", ".", "NETWORK", "--unknown"],
            out _);

        Assert.False(result);
    }

    [Fact]
    public void TryParseOptions_WithDuplicatePlainSwitch_ReturnsFalse()
    {
        bool result = ShowCodeGroupCommand.TryParseOptions(
            ["show-code-group", ".", "NETWORK", "--plain", "--plain"],
            out _);

        Assert.False(result);
    }

    [Fact]
    public void FindCodeGroup_ByName_ReturnsMatchingCodeGroup()
    {
        WhenItFailsWorkspaceSummary summary = CreateSummary();

        ErrorCodeGroupDefinition? result = ShowCodeGroupCommand.FindCodeGroup(summary, "NETWORK");

        Assert.NotNull(result);
        Assert.Equal("NET", result.CodePrefix);
    }

    [Fact]
    public void FindCodeGroup_ByDisplayName_ReturnsMatchingCodeGroup()
    {
        WhenItFailsWorkspaceSummary summary = CreateSummary();

        ErrorCodeGroupDefinition? result = ShowCodeGroupCommand.FindCodeGroup(summary, "Network");

        Assert.NotNull(result);
        Assert.Equal("NETWORK", result.Name);
    }

    [Fact]
    public void FindCodeGroup_ByPrefix_ReturnsMatchingCodeGroup()
    {
        WhenItFailsWorkspaceSummary summary = CreateSummary();

        ErrorCodeGroupDefinition? result = ShowCodeGroupCommand.FindCodeGroup(summary, "net");

        Assert.NotNull(result);
        Assert.Equal("NETWORK", result.Name);
    }

    [Fact]
    public void FindCodeGroup_WithUnknownValue_ReturnsNull()
    {
        WhenItFailsWorkspaceSummary summary = CreateSummary();

        ErrorCodeGroupDefinition? result = ShowCodeGroupCommand.FindCodeGroup(summary, "UNKNOWN");

        Assert.Null(result);
    }

    private static WhenItFailsWorkspaceSummary CreateSummary()
    {
        return new WhenItFailsWorkspaceSummary
        {
            CodeGroupCatalog = new ErrorCodeGroupCatalogDocument
            {
                CodeGroups =
                [
                    new ErrorCodeGroupDefinition
                    {
                        Name = "NETWORK",
                        DisplayName = "Network",
                        CodePrefix = "NET",
                        CodeFrom = 100000,
                        CodeTo = 199999
                    }
                ]
            }
        };
    }
}
