using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Validation;

public sealed class ErrorProfileCatalogValidatorNullProfileDefinitionContractTests
{
    [Fact]
    public void Validate_ShouldReturnInvalidResult_WhenProfileCollectionContainsNullDefinition()
    {
        ErrorProfileCatalogValidator validator = new();

        ErrorProfileCatalogDocument document = new()
        {
            Profiles = [null!]
        };

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.False(result.IsValid);

        var issue = Assert.Single(
            result.Issues,
            issue => issue.Code == "ProfileDefinitionIsNull");

        Assert.Equal("profiles[0]", issue.Path);
        Assert.Equal(
            "Profile catalog contains a null profile definition.",
            issue.Message);
    }
}
