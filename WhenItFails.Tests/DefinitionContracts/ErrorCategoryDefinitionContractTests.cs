using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.WhenItFails.Tests.DefinitionContracts;

public sealed class ErrorCategoryDefinitionContractTests
{
    [Fact]
    public void NewDefinition_ShouldUseSafeDefaults()
    {
        ErrorCategoryDefinition definition = new();

        Assert.Equal(string.Empty, definition.Name);
        Assert.Equal(string.Empty, definition.DisplayName);
        Assert.Null(definition.Description);
        Assert.Empty(definition.Aliases);
        Assert.Empty(definition.ParentCategories);
        Assert.Empty(definition.DefaultTags);
        Assert.Empty(definition.DefaultMappings);
        Assert.NotNull(definition.Metadata);
    }

    [Fact]
    public void NewDefinitions_ShouldNotShareMutableContainers()
    {
        ErrorCategoryDefinition first = new();
        ErrorCategoryDefinition second = new();

        Assert.NotSame(first.Aliases, second.Aliases);
        Assert.NotSame(first.ParentCategories, second.ParentCategories);
        Assert.NotSame(first.DefaultTags, second.DefaultTags);
        Assert.NotSame(first.DefaultMappings, second.DefaultMappings);
        Assert.NotSame(first.Metadata, second.Metadata);
    }
}
