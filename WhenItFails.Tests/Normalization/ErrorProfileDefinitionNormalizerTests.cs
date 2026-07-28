using Afrowave.Toolbox.Essentials.Metadata;
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
            Source = " imported ",
            IncludeOwners = ["afw", "AFW"],
            IncludeCodeGroups = ["configuration", "validation"],
            IncludeCategories = ["web", "server", "web"],
            IncludeSubcategories = ["required value", "required-value"],
            IncludeTags = ["user visible", "user-visible"],
            IncludeErrors = ["afw_net_0001", "AFW_NET_0001", " afw_cfg_0001 "],
            ExcludeTags = ["internal only", "internal-only"],
            ExcludeErrors = ["afw_dbg_0001", "AFW_DBG_0001"],
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
        Assert.Equal(["AFW_NET_0001", "AFW_CFG_0001"], normalizedDefinition.IncludeErrors);
        Assert.Equal(["AFW_DBG_0001"], normalizedDefinition.ExcludeErrors);

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
            ExcludeTags = ["", "internal only"],
            IncludeErrors = ["", "afw_net_0001"],
            ExcludeErrors = ["", "afw_dbg_0001"]
        };

        ErrorProfileDefinition normalizedDefinition = normalizer.Normalize(definition);

        Assert.Equal(["AFW"], normalizedDefinition.IncludeOwners);
        Assert.Equal(["CONFIGURATION"], normalizedDefinition.IncludeCodeGroups);
        Assert.Equal(["WEB"], normalizedDefinition.IncludeCategories);
        Assert.Equal(["REQUIRED_VALUE"], normalizedDefinition.IncludeSubcategories);
        Assert.Equal(["USER_VISIBLE"], normalizedDefinition.IncludeTags);
        Assert.Equal(["INTERNAL_ONLY"], normalizedDefinition.ExcludeTags);
        Assert.Equal(["AFW_NET_0001"], normalizedDefinition.IncludeErrors);
        Assert.Equal(["AFW_DBG_0001"], normalizedDefinition.ExcludeErrors);
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
    public void Normalize_ShouldCopyDefaultMappingsWithoutSharingMutableState()
    {
        ErrorProfileDefinitionNormalizer normalizer = new();
        ErrorProfileDefinition definition = new()
        {
            DefaultMappings =
            {
                ["web.problemDetails"] = " true ",
                ["web.includeTraceId"] = " false "
            }
        };

        ErrorProfileDefinition normalizedDefinition = normalizer.Normalize(definition);

        Assert.NotSame(definition.DefaultMappings, normalizedDefinition.DefaultMappings);
        Assert.Equal("true", normalizedDefinition.DefaultMappings["WEB_PROBLEMDETAILS"]);
        Assert.Equal("false", normalizedDefinition.DefaultMappings["WEB_INCLUDETRACEID"]);

        normalizedDefinition.DefaultMappings["WEB_PROBLEMDETAILS"] = "false";
        normalizedDefinition.DefaultMappings["RUNTIME_ONLY"] = "true";

        Assert.Equal(" true ", definition.DefaultMappings["web.problemDetails"]);
        Assert.False(definition.DefaultMappings.ContainsKey("RUNTIME_ONLY"));
    }

    [Fact]
    public void Normalize_ShouldCopyMetadataWithoutSharingMutableState()
    {
        ErrorProfileDefinitionNormalizer normalizer = new();
        ErrorProfileDefinition definition = new()
        {
            Metadata = new MetadataBag(
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["consumer"] = "SeeMe",
                    ["auditNote"] = "preserve independently"
                })
        };

        ErrorProfileDefinition normalizedDefinition = normalizer.Normalize(definition);

        Assert.NotSame(definition.Metadata, normalizedDefinition.Metadata);
        Assert.Equal(definition.Metadata.Items, normalizedDefinition.Metadata.Items);

        normalizedDefinition.Metadata.Set("consumer", "Changed");
        normalizedDefinition.Metadata.Set("newValue", "runtime-only");

        Assert.Equal("SeeMe", definition.Metadata["consumer"]);
        Assert.False(definition.Metadata.TryGet("newValue", out _));
    }

    [Fact]
    public void Normalize_ShouldNotModifyOriginalDefinition()
    {
        ErrorProfileDefinitionNormalizer normalizer = new();

        ErrorProfileDefinition definition = new()
        {
            Name = " web api ",
            DisplayName = " Web API ",
            Source = " imported ",
            IncludeOwners = ["afw"],
            IncludeTags = ["user visible"],
            IncludeErrors = ["afw_net_0001"],
            ExcludeErrors = ["afw_dbg_0001"]
        };

        ErrorProfileDefinition normalizedDefinition = normalizer.Normalize(definition);

        Assert.Equal(" web api ", definition.Name);
        Assert.Equal(" Web API ", definition.DisplayName);
        Assert.Equal(["afw"], definition.IncludeOwners);
        Assert.Equal(["user visible"], definition.IncludeTags);
        Assert.Equal(" imported ", definition.Source);
        Assert.Equal(["afw_net_0001"], definition.IncludeErrors);
        Assert.Equal(["afw_dbg_0001"], definition.ExcludeErrors);

        Assert.Equal("WEB_API", normalizedDefinition.Name);
        Assert.Equal("Web API", normalizedDefinition.DisplayName);
        Assert.Equal(["AFW"], normalizedDefinition.IncludeOwners);
        Assert.Equal(["USER_VISIBLE"], normalizedDefinition.IncludeTags);
        Assert.Equal("imported", normalizedDefinition.Source);
        Assert.Equal(["AFW_NET_0001"], normalizedDefinition.IncludeErrors);
        Assert.Equal(["AFW_DBG_0001"], normalizedDefinition.ExcludeErrors);
    }

    [Fact]
    public void Normalize_ShouldKeepDefaultSource_WhenSourceIsNotChanged()
    {
        ErrorProfileDefinitionNormalizer normalizer = new();

        ErrorProfileDefinition definition = new();

        ErrorProfileDefinition normalizedDefinition = normalizer.Normalize(definition);

        Assert.Equal("Project", normalizedDefinition.Source);
    }
}
