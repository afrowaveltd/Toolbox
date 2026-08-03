using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.WhenItFails.Tests.DefinitionContracts;

public sealed class ErrorProfileDefinitionContractTests
{
    [Fact]
    public void NewProfile_ShouldUseSafeDefaults()
    {
        ErrorProfileDefinition profile = new();

        Assert.Equal(string.Empty, profile.Name);
        Assert.Equal(string.Empty, profile.DisplayName);
        Assert.Null(profile.Description);
        Assert.Equal("Project", profile.Source);
        Assert.Empty(profile.IncludeOwners);
        Assert.Empty(profile.IncludeCodeGroups);
        Assert.Empty(profile.IncludeCategories);
        Assert.Empty(profile.IncludeSubcategories);
        Assert.Empty(profile.IncludeTags);
        Assert.Empty(profile.IncludeErrors);
        Assert.Empty(profile.ExcludeTags);
        Assert.Empty(profile.ExcludeErrors);
        Assert.Empty(profile.DefaultMappings);
        Assert.NotNull(profile.Metadata);
    }

    [Fact]
    public void NewProfiles_ShouldNotShareMutableContainers()
    {
        ErrorProfileDefinition first = new();
        ErrorProfileDefinition second = new();

        Assert.NotSame(first.IncludeOwners, second.IncludeOwners);
        Assert.NotSame(first.IncludeCodeGroups, second.IncludeCodeGroups);
        Assert.NotSame(first.IncludeCategories, second.IncludeCategories);
        Assert.NotSame(first.IncludeSubcategories, second.IncludeSubcategories);
        Assert.NotSame(first.IncludeTags, second.IncludeTags);
        Assert.NotSame(first.IncludeErrors, second.IncludeErrors);
        Assert.NotSame(first.ExcludeTags, second.ExcludeTags);
        Assert.NotSame(first.ExcludeErrors, second.ExcludeErrors);
        Assert.NotSame(first.DefaultMappings, second.DefaultMappings);
        Assert.NotSame(first.Metadata, second.Metadata);
    }
}
