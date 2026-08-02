using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.WhenItFails.Tests.DefinitionContracts;

public sealed class ErrorDefinitionContractTests
{
    [Fact]
    public void NewDefinition_ShouldUseSafeDefaults()
    {
        ErrorDefinition definition = new();

        Assert.Equal(string.Empty, definition.Id);
        Assert.Equal(0, definition.Code);
        Assert.Equal(string.Empty, definition.Name);
        Assert.Equal(string.Empty, definition.Owner);
        Assert.Equal(string.Empty, definition.CodePrefix);
        Assert.Equal(string.Empty, definition.CodeGroup);
        Assert.Equal(string.Empty, definition.PrimaryCategory);
        Assert.Empty(definition.Categories);
        Assert.Empty(definition.Subcategories);
        Assert.Equal(string.Empty, definition.Title);
        Assert.Equal(string.Empty, definition.Message);
        Assert.Equal("Error", definition.DefaultSeverity);
        Assert.Null(definition.DeveloperHint);
        Assert.Null(definition.DocumentationKey);
        Assert.Empty(definition.Tags);
        Assert.NotNull(definition.Metadata);
    }

    [Fact]
    public void NewDefinitions_ShouldNotShareMutableContainers()
    {
        ErrorDefinition first = new();
        ErrorDefinition second = new();

        Assert.NotSame(first.Categories, second.Categories);
        Assert.NotSame(first.Subcategories, second.Subcategories);
        Assert.NotSame(first.Tags, second.Tags);
        Assert.NotSame(first.Metadata, second.Metadata);
    }
}
