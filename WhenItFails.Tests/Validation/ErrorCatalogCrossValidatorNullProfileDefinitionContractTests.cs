using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Validation;

public sealed class ErrorCatalogCrossValidatorNullProfileDefinitionContractTests
{
    [Fact]
    public void Validate_ShouldReturnInvalidResult_WhenProfileCatalogContainsNullDefinition()
    {
        ErrorCatalogCrossValidator validator = new();

        ErrorProfileCatalogDocument profileCatalog = new()
        {
            Profiles = [null!]
        };

        ErrorCatalogValidationResult result = validator.Validate(
            new ErrorCatalogDocument(),
            new ErrorOwnerCatalogDocument(),
            new ErrorCodeGroupCatalogDocument(),
            new ErrorCategoryCatalogDocument(),
            profileCatalog);

        Assert.False(result.IsValid);

        var issue = Assert.Single(
            result.Issues,
            issue => issue.Code == "ProfileDefinitionIsNull");

        Assert.Equal("profileCatalog.profiles[0]", issue.Path);
        Assert.Equal(
            "Profile catalog contains a null profile definition.",
            issue.Message);
    }
}
