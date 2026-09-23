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

}
