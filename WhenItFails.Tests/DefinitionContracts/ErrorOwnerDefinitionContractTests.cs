using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.WhenItFails.Tests.DefinitionContracts;

public sealed class ErrorOwnerDefinitionContractTests
{
    [Fact]
    public void NewOwner_ShouldUseSafeDefaults()
    {
        ErrorOwnerDefinition owner = new();

        Assert.Equal(string.Empty, owner.Name);
        Assert.Equal(string.Empty, owner.DisplayName);
        Assert.Null(owner.Description);
        Assert.Equal(0, owner.CodeFrom);
        Assert.Equal(0, owner.CodeTo);
        Assert.False(owner.IsBuiltIn);
        Assert.Empty(owner.Aliases);
        Assert.Empty(owner.DefaultMappings);
        Assert.NotNull(owner.Metadata);
    }

    [Fact]
    public void NewOwners_ShouldNotShareMutableContainers()
    {
        ErrorOwnerDefinition first = new();
        ErrorOwnerDefinition second = new();

        Assert.NotSame(first.Aliases, second.Aliases);
        Assert.NotSame(first.DefaultMappings, second.DefaultMappings);
        Assert.NotSame(first.Metadata, second.Metadata);
    }
}
