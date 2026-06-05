using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.WhenItFails.Tests.Normalization;

public sealed class ErrorProfileDefinitionNormalizerTests
{
    [Fact]
    public void Normalize_ShouldThrowArgumentNullException_WhenDefinitionIsNull()
    {
        ErrorProfileDefinitionNormalizer normalizer = new();

        Assert.Throws<ArgumentNullException>(() => normalizer.Normalize(null!));
    }

    [Fact]
    public void Normalize_ShouldNormalizeBasicFields()
    {
        ErrorProfileDefinitionNormalizer normalizer = new();

        ErrorProfileDefinition definition = new()
        {
            Name = " web api ",
            DisplayName = " Web API ",
            Description = " Profile for web APIs. ",
            IncludeOwners = ["afw", "AFW"],
            IncludeCodeGroups = ["configuration", "validation"],
            IncludeCategories = ["web", "server", "web"],
            IncludeSubcategories = ["required value", "required-value"],
            IncludeTags = ["user visible", "user-visible"],
            ExcludeTags = ["internal only", "internal-only"],
            DefaultMappings =
            {
                ["web.problemDetails"] = " true ",
                ["production.includeExceptionDetails"] = " false "
            }
        };

        ErrorProfileDefinition normalizedDefinition = normalizer.Normalize(definition);

        Assert.Equal("WEB_API", normalizedDefinition.Name);
        Assert.Equal("Web API", normalizedDefinition.DisplayName);
        Assert.Equal("Profile for web APIs.", normalizedDefinition.Description);

        Assert.Equal(["AFW"], normalizedDefinition.IncludeOwners);
        Assert.Equal(["CONFIGURATION", "VALIDATION"], normalizedDefinition.IncludeCodeGroups);
        Assert.Equal(["WEB", "SERVER"], normalizedDefinition.IncludeCategories);
        Assert.Equal(["REQUIRED_VALUE"], normalizedDefinition.IncludeSubcategories);
        Assert.Equal(["USER_VISIBLE"], normalizedDefinition.IncludeTags);
        Assert.Equal(["INTERNAL_ONLY"], normalizedDefinition.ExcludeTags);

        Assert.Equal("true", normalizedDefinition.DefaultMappings["WEB_PROBLEMDETAILS"]);
        Assert.Equal("false", normalizedDefinition.DefaultMappings["PRODUCTION_INCLUDEEXCEPTIONDETAILS"]);
    }

    [Fact]
    public void Normalize_ShouldUseNameAsDisplayNameFallback_WhenDisplayNameIsEmpty()
    {
        ErrorProfileDefinitionNormalizer normalizer = new();

        ErrorProfileDefinition definition = new()
        {
            Name = "web api",
            DisplayName = " "
        };

        ErrorProfileDefinition normalizedDefinition = normalizer.Normalize(definition);

        Assert.Equal("WEB_API", normalizedDefinition.Name);
        Assert.Equal("web api", normalizedDefinition.DisplayName);
    }

    [Fact]
    public void Normalize_ShouldRemoveEmptyCollectionValues()
    {
        ErrorProfileDefinitionNormalizer normalizer = new();

        ErrorProfileDefinition definition = new()
        {
            IncludeOwners = ["", "afw"],
            IncludeCodeGroups = ["", "configuration"],
            IncludeCategories = ["", "web"],
            IncludeSubcategories = ["", "required value"],
            IncludeTags = ["", "user visible"],
            ExcludeTags = ["", "internal only"]
        };

        ErrorProfileDefinition normalizedDefinition = normalizer.Normalize(definition);

        Assert.Equal(["AFW"], normalizedDefinition.IncludeOwners);
        Assert.Equal(["CONFIGURATION"], normalizedDefinition.IncludeCodeGroups);
        Assert.Equal(["WEB"], normalizedDefinition.IncludeCategories);
        Assert.Equal(["REQUIRED_VALUE"], normalizedDefinition.IncludeSubcategories);
        Assert.Equal(["USER_VISIBLE"], normalizedDefinition.IncludeTags);
        Assert.Equal(["INTERNAL_ONLY"], normalizedDefinition.ExcludeTags);
    }

    [Fact]
    public void Normalize_ShouldIgnoreEmptyMappingKeys()
    {
        ErrorProfileDefinitionNormalizer normalizer = new();

        ErrorProfileDefinition definition = new()
        {
            DefaultMappings =
            {
                [""] = "ignored",
                [" "] = "ignored",
                ["web.problemDetails"] = " true "
            }
        };

        ErrorProfileDefinition normalizedDefinition = normalizer.Normalize(definition);

        Assert.Single(normalizedDefinition.DefaultMappings);
        Assert.Equal("true", normalizedDefinition.DefaultMappings["WEB_PROBLEMDETAILS"]);
    }

    [Fact]
    public void Normalize_ShouldKeepMetadataReference()
    {
        ErrorProfileDefinitionNormalizer normalizer = new();

        ErrorProfileDefinition definition = new();

        ErrorProfileDefinition normalizedDefinition = normalizer.Normalize(definition);

        Assert.Same(definition.Metadata, normalizedDefinition.Metadata);
    }

    [Fact]
    public void Normalize_ShouldNotModifyOriginalDefinition()
    {
        ErrorProfileDefinitionNormalizer normalizer = new();

        ErrorProfileDefinition definition = new()
        {
            Name = " web api ",
            DisplayName = " Web API ",
            IncludeOwners = ["afw"],
            IncludeTags = ["user visible"]
        };

        ErrorProfileDefinition normalizedDefinition = normalizer.Normalize(definition);

        Assert.Equal(" web api ", definition.Name);
        Assert.Equal(" Web API ", definition.DisplayName);
        Assert.Equal(["afw"], definition.IncludeOwners);
        Assert.Equal(["user visible"], definition.IncludeTags);

        Assert.Equal("WEB_API", normalizedDefinition.Name);
        Assert.Equal("Web API", normalizedDefinition.DisplayName);
        Assert.Equal(["AFW"], normalizedDefinition.IncludeOwners);
        Assert.Equal(["USER_VISIBLE"], normalizedDefinition.IncludeTags);
    }
}