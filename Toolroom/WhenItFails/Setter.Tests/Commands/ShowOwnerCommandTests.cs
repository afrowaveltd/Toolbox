using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

public sealed class ShowOwnerCommandTests
{
    [Fact]
    public void TryParseOptions_WithoutOptionalSwitch_ReturnsRichOutput()
    {
        bool result = ShowOwnerCommand.TryParseOptions(
            ["show-owner", ".", "AFW"],
            out bool usePlainOutput,
            out bool useJsonOutput);

        Assert.True(result);
        Assert.False(usePlainOutput);
        Assert.False(useJsonOutput);
    }

    [Fact]
    public void TryParseOptions_WithPlainSwitch_ReturnsPlainOutput()
    {
        bool result = ShowOwnerCommand.TryParseOptions(
            ["show-owner", ".", "AFW", "--plain"],
            out bool usePlainOutput,
            out bool useJsonOutput);

        Assert.True(result);
        Assert.True(usePlainOutput);
        Assert.False(useJsonOutput);
    }

    [Fact]
    public void TryParseOptions_WithJsonSwitch_ReturnsJsonOutput()
    {
        bool result = ShowOwnerCommand.TryParseOptions(
            ["show-owner", ".", "AFW", "--JSON"],
            out bool usePlainOutput,
            out bool useJsonOutput);

        Assert.True(result);
        Assert.False(usePlainOutput);
        Assert.True(useJsonOutput);
    }

    [Fact]
    public void TryParseOptions_WithUnknownArgument_ReturnsFalse()
    {
        bool result = ShowOwnerCommand.TryParseOptions(
            ["show-owner", ".", "AFW", "--unknown"],
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
        bool result = ShowOwnerCommand.TryParseOptions(
            ["show-owner", ".", "AFW", first, second],
            out _,
            out _);

        Assert.False(result);
    }

    [Fact]
    public void FindOwner_ByName_ReturnsMatchingOwner()
    {
        WhenItFailsWorkspaceSummary summary = CreateSummary();

        ErrorOwnerDefinition? result = ShowOwnerCommand.FindOwner(summary, "AFW");

        Assert.NotNull(result);
        Assert.Equal("Afrowave", result.DisplayName);
    }

    [Fact]
    public void FindOwner_ByDisplayName_ReturnsMatchingOwner()
    {
        WhenItFailsWorkspaceSummary summary = CreateSummary();

        ErrorOwnerDefinition? result = ShowOwnerCommand.FindOwner(summary, "Afrowave");

        Assert.NotNull(result);
        Assert.Equal("AFW", result.Name);
    }

    [Fact]
    public void FindOwner_ByAlias_ReturnsMatchingOwner()
    {
        WhenItFailsWorkspaceSummary summary = CreateSummary();

        ErrorOwnerDefinition? result = ShowOwnerCommand.FindOwner(summary, "official");

        Assert.NotNull(result);
        Assert.Equal("AFW", result.Name);
    }

    [Fact]
    public void FindOwner_WithUnknownValue_ReturnsNull()
    {
        WhenItFailsWorkspaceSummary summary = CreateSummary();

        ErrorOwnerDefinition? result = ShowOwnerCommand.FindOwner(summary, "UNKNOWN");

        Assert.Null(result);
    }

    private static WhenItFailsWorkspaceSummary CreateSummary()
    {
        return new WhenItFailsWorkspaceSummary
        {
            OwnerCatalog = new ErrorOwnerCatalogDocument
            {
                Owners =
                [
                    new ErrorOwnerDefinition
                    {
                        Name = "AFW",
                        DisplayName = "Afrowave",
                        CodeFrom = 0,
                        CodeTo = 999999,
                        IsBuiltIn = true,
                        Aliases = ["OFFICIAL"]
                    }
                ]
            }
        };
    }
}
