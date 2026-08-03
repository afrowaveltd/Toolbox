using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.WhenItFails.Tests.DefinitionContracts;

public sealed class ErrorCodeGroupDefinitionContractTests
{
    [Fact]
    public void NewDefinition_ShouldUseSafeDefaults()
    {
        ErrorCodeGroupDefinition definition = new();

        Assert.Equal(string.Empty, definition.Name);
        Assert.Equal(string.Empty, definition.DisplayName);
        Assert.Equal(string.Empty, definition.CodePrefix);
        Assert.Equal(0, definition.CodeFrom);
        Assert.Equal(0, definition.CodeTo);
        Assert.Null(definition.Description);
        Assert.Empty(definition.DefaultCategories);
        Assert.Empty(definition.DefaultTags);
        Assert.Empty(definition.DefaultMappings);
        Assert.NotNull(definition.Metadata);
    }

    [Fact]
    public void NewDefinitions_ShouldNotShareMutableContainers()
    {
        ErrorCodeGroupDefinition first = new();
        ErrorCodeGroupDefinition second = new();

        Assert.NotSame(first.DefaultCategories, second.DefaultCategories);
        Assert.NotSame(first.DefaultTags, second.DefaultTags);
        Assert.NotSame(first.DefaultMappings, second.DefaultMappings);
        Assert.NotSame(first.Metadata, second.Metadata);
    }
}
