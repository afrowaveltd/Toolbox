using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

public sealed class ShowOwnerCommandTests
{
    [Fact]
    public void TryParseOptions_WithoutOptionalSwitch_ReturnsTrueAndPlainFalse()
    {
        bool result = ShowOwnerCommand.TryParseOptions(
            ["show-owner", ".", "AFW"],
            out bool usePlainOutput);

        Assert.True(result);
        Assert.False(usePlainOutput);
    }

    [Fact]
    public void TryParseOptions_WithPlainSwitch_ReturnsTrueAndPlainTrue()
    {
        bool result = ShowOwnerCommand.TryParseOptions(
            ["show-owner", ".", "AFW", "--plain"],
            out bool usePlainOutput);

        Assert.True(result);
        Assert.True(usePlainOutput);
    }

    [Fact]
    public void TryParseOptions_WithCaseInsensitivePlainSwitch_ReturnsTrue()
    {
        bool result = ShowOwnerCommand.TryParseOptions(
            ["show-owner", ".", "AFW", "--PLAIN"],
            out bool usePlainOutput);

        Assert.True(result);
        Assert.True(usePlainOutput);
    }

    [Fact]
    public void TryParseOptions_WithUnknownArgument_ReturnsFalse()
    {
        bool result = ShowOwnerCommand.TryParseOptions(
            ["show-owner", ".", "AFW", "--unknown"],
            out _);

        Assert.False(result);
    }

    [Fact]
    public void TryParseOptions_WithDuplicatePlainSwitch_ReturnsFalse()
    {
        bool result = ShowOwnerCommand.TryParseOptions(
            ["show-owner", ".", "AFW", "--plain", "--plain"],
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
