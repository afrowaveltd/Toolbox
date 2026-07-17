using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

public sealed class ShowCodeGroupCommandTests
{
    [Fact]
    public void TryParseOptions_WithoutOptionalSwitch_ReturnsRichOutput()
    {
        bool result = ShowCodeGroupCommand.TryParseOptions(
            ["show-code-group", ".", "NETWORK"],
            out bool usePlainOutput,
            out bool useJsonOutput);

        Assert.True(result);
        Assert.False(usePlainOutput);
        Assert.False(useJsonOutput);
    }

    [Fact]
    public void TryParseOptions_WithPlainSwitch_ReturnsPlainOutput()
    {
        bool result = ShowCodeGroupCommand.TryParseOptions(
            ["show-code-group", ".", "NETWORK", "--plain"],
            out bool usePlainOutput,
            out bool useJsonOutput);

        Assert.True(result);
        Assert.True(usePlainOutput);
        Assert.False(useJsonOutput);
    }

    [Theory]
    [InlineData("--json")]
    [InlineData("--JSON")]
    public void TryParseOptions_WithJsonSwitch_ReturnsJsonOutput(string jsonSwitch)
    {
        bool result = ShowCodeGroupCommand.TryParseOptions(
            ["show-code-group", ".", "NETWORK", jsonSwitch],
            out bool usePlainOutput,
            out bool useJsonOutput);

        Assert.True(result);
        Assert.False(usePlainOutput);
        Assert.True(useJsonOutput);
    }

    [Theory]
    [InlineData("--unknown")]
    [InlineData("unexpected")]
    public void TryParseOptions_WithUnknownArgument_ReturnsFalse(string unknownArgument)
    {
        bool result = ShowCodeGroupCommand.TryParseOptions(
            ["show-code-group", ".", "NETWORK", unknownArgument],
            out _,
            out _);

        Assert.False(result);
    }

    [Theory]
    [InlineData("--plain", "--plain")]
    [InlineData("--json", "--json")]
    [InlineData("--plain", "--json")]
    public void TryParseOptions_WithDuplicateOrConflictingSwitches_ReturnsFalse(
        string first,
        string second)
    {
        bool result = ShowCodeGroupCommand.TryParseOptions(
            ["show-code-group", ".", "NETWORK", first, second],
            out _,
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
