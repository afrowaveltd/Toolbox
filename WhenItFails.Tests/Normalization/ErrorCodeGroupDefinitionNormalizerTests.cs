using Afrowave.Toolbox.Essentials.Metadata;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.WhenItFails.Tests.Normalization;

public sealed class ErrorCodeGroupDefinitionNormalizerTests
{
    [Fact]
    public void Normalize_ShouldThrowArgumentNullException_WhenDefinitionIsNull()
    {
        ErrorCodeGroupDefinitionNormalizer normalizer = new();

        Assert.Throws<ArgumentNullException>(
            () => normalizer.Normalize(null!));
    }

    [Fact]
    public void Normalize_ShouldNormalizeBasicFields()
    {
        ErrorCodeGroupDefinitionNormalizer normalizer = new();

        ErrorCodeGroupDefinition definition = new()
        {
            Name = " configuration ",
            DisplayName = " Configuration errors ",
            CodePrefix = " cfg ",
            CodeFrom = 200000,
            CodeTo = 299999,
            Description = " Configuration failures. ",
            DefaultCategories = ["configuration", "CONFIGURATION"],
            DefaultTags = ["system", "SYSTEM"],
            DefaultMappings =
            {
                ["web.httpStatusCode"] = " 500 ",
                ["retry.enabled"] = " false "
            }
        };

        ErrorCodeGroupDefinition normalizedDefinition =
            normalizer.Normalize(definition);

        Assert.Equal("CONFIGURATION", normalizedDefinition.Name);
        Assert.Equal("Configuration errors", normalizedDefinition.DisplayName);
        Assert.Equal("CFG", normalizedDefinition.CodePrefix);
        Assert.Equal(200000, normalizedDefinition.CodeFrom);
        Assert.Equal(299999, normalizedDefinition.CodeTo);
        Assert.Equal("Configuration failures.", normalizedDefinition.Description);
        Assert.Equal(["CONFIGURATION"], normalizedDefinition.DefaultCategories);
        Assert.Equal(["SYSTEM"], normalizedDefinition.DefaultTags);
        Assert.Equal("500", normalizedDefinition.DefaultMappings["WEB_HTTPSTATUSCODE"]);
        Assert.Equal("false", normalizedDefinition.DefaultMappings["RETRY_ENABLED"]);
    }


    [Fact]
    public void Normalize_ShouldCopyCollectionsAndMappingsWithoutSharingMutableState()
    {
        ErrorCodeGroupDefinitionNormalizer normalizer = new();

        ErrorCodeGroupDefinition definition = new()
        {
            DefaultCategories = ["configuration"],
            DefaultTags = ["system"],
            DefaultMappings =
            {
                ["web.httpStatusCode"] = " 500 "
            }
        };

        ErrorCodeGroupDefinition normalizedDefinition =
            normalizer.Normalize(definition);

        Assert.NotSame(definition.DefaultCategories, normalizedDefinition.DefaultCategories);
        Assert.NotSame(definition.DefaultTags, normalizedDefinition.DefaultTags);
        Assert.NotSame(definition.DefaultMappings, normalizedDefinition.DefaultMappings);

        normalizedDefinition.DefaultCategories.Add("VALIDATION");
        normalizedDefinition.DefaultTags[0] = "RUNTIME_ONLY";
        normalizedDefinition.DefaultMappings["WEB_HTTPSTATUSCODE"] = "503";
        normalizedDefinition.DefaultMappings["RUNTIME_ONLY"] = "true";

        Assert.Equal(["configuration"], definition.DefaultCategories);
        Assert.Equal(["system"], definition.DefaultTags);
        Assert.Equal(" 500 ", definition.DefaultMappings["web.httpStatusCode"]);
        Assert.False(definition.DefaultMappings.ContainsKey("RUNTIME_ONLY"));
    }


    [Fact]
    public void Normalize_ShouldCopyMetadataWithoutSharingMutableState()
    {
        ErrorCodeGroupDefinitionNormalizer normalizer = new();

        ErrorCodeGroupDefinition definition = new()
        {
            Metadata = new MetadataBag(
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["consumer"] = "WhenItFails",
                    ["auditNote"] = "preserve independently"
                })
        };

        ErrorCodeGroupDefinition normalizedDefinition =
            normalizer.Normalize(definition);

        Assert.NotSame(definition.Metadata, normalizedDefinition.Metadata);
        Assert.Equal(definition.Metadata.Items, normalizedDefinition.Metadata.Items);

        normalizedDefinition.Metadata.Set("consumer", "Changed");
        normalizedDefinition.Metadata.Set("newValue", "runtime-only");

        Assert.Equal("WhenItFails", definition.Metadata["consumer"]);
        Assert.False(definition.Metadata.TryGet("newValue", out _));
    }

}
