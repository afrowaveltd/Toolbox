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

}
