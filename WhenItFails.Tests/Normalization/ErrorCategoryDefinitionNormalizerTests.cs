using Afrowave.Toolbox.Essentials.Metadata;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.WhenItFails.Tests.Normalization;

public sealed class ErrorCategoryDefinitionNormalizerTests
{
    [Fact]
    public void Normalize_ShouldThrowArgumentNullException_WhenDefinitionIsNull()
    {
        ErrorCategoryDefinitionNormalizer normalizer = new();

        Assert.Throws<ArgumentNullException>(
            () => normalizer.Normalize(null!));
    }

    [Fact]
    public void Normalize_ShouldNormalizeBasicFields()
    {
        ErrorCategoryDefinitionNormalizer normalizer = new();

        ErrorCategoryDefinition definition = new()
        {
            Name = " external service ",
            DisplayName = " External service ",
            Description = " Calls to external systems. ",
            Aliases = ["external-service", "EXTERNAL SERVICE"],
            ParentCategories = ["integration", "INTEGRATION"],
            DefaultTags = ["user visible", "USER_VISIBLE"],
            DefaultMappings =
            {
                ["web.httpStatusCode"] = " 503 ",
                ["retry.enabled"] = " true "
            }
        };

        ErrorCategoryDefinition normalizedDefinition =
            normalizer.Normalize(definition);

        Assert.Equal("EXTERNAL_SERVICE", normalizedDefinition.Name);
        Assert.Equal("External service", normalizedDefinition.DisplayName);
        Assert.Equal("Calls to external systems.", normalizedDefinition.Description);
        Assert.Equal(["EXTERNAL_SERVICE"], normalizedDefinition.Aliases);
        Assert.Equal(["INTEGRATION"], normalizedDefinition.ParentCategories);
        Assert.Equal(["USER_VISIBLE"], normalizedDefinition.DefaultTags);
        Assert.Equal("503", normalizedDefinition.DefaultMappings["WEB_HTTPSTATUSCODE"]);
        Assert.Equal("true", normalizedDefinition.DefaultMappings["RETRY_ENABLED"]);
    }


    [Fact]
    public void Normalize_ShouldCopyCollectionsAndMappingsWithoutSharingMutableState()
    {
        ErrorCategoryDefinitionNormalizer normalizer = new();

        ErrorCategoryDefinition definition = new()
        {
            Aliases = ["external service"],
            ParentCategories = ["integration"],
            DefaultTags = ["user visible"],
            DefaultMappings =
            {
                ["web.httpStatusCode"] = " 503 "
            }
        };

        ErrorCategoryDefinition normalizedDefinition =
            normalizer.Normalize(definition);

        Assert.NotSame(definition.Aliases, normalizedDefinition.Aliases);
        Assert.NotSame(definition.ParentCategories, normalizedDefinition.ParentCategories);
        Assert.NotSame(definition.DefaultTags, normalizedDefinition.DefaultTags);
        Assert.NotSame(definition.DefaultMappings, normalizedDefinition.DefaultMappings);

        normalizedDefinition.Aliases.Add("SECONDARY");
        normalizedDefinition.ParentCategories.Clear();
        normalizedDefinition.DefaultTags[0] = "RUNTIME_ONLY";
        normalizedDefinition.DefaultMappings["WEB_HTTPSTATUSCODE"] = "500";
        normalizedDefinition.DefaultMappings["RUNTIME_ONLY"] = "true";

        Assert.Equal(["external service"], definition.Aliases);
        Assert.Equal(["integration"], definition.ParentCategories);
        Assert.Equal(["user visible"], definition.DefaultTags);
        Assert.Equal(" 503 ", definition.DefaultMappings["web.httpStatusCode"]);
        Assert.False(definition.DefaultMappings.ContainsKey("RUNTIME_ONLY"));
    }


    [Fact]
    public void Normalize_ShouldCopyMetadataWithoutSharingMutableState()
    {
        ErrorCategoryDefinitionNormalizer normalizer = new();

        ErrorCategoryDefinition definition = new()
        {
            Metadata = new MetadataBag(
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["consumer"] = "SeeMe",
                    ["auditNote"] = "preserve independently"
                })
        };

        ErrorCategoryDefinition normalizedDefinition =
            normalizer.Normalize(definition);

        Assert.NotSame(definition.Metadata, normalizedDefinition.Metadata);
        Assert.Equal(definition.Metadata.Items, normalizedDefinition.Metadata.Items);

        normalizedDefinition.Metadata.Set("consumer", "Changed");
        normalizedDefinition.Metadata.Set("newValue", "runtime-only");

        Assert.Equal("SeeMe", definition.Metadata["consumer"]);
        Assert.False(definition.Metadata.TryGet("newValue", out _));
    }

}
