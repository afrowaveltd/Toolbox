using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Validation;

public sealed class ErrorCatalogCrossValidatorNullProfileExcludeErrorsCollectionContractTests
{
    [Fact]
    public void Validate_ShouldReturnInvalidResult_WhenProfileExcludeErrorsCollectionIsNull()
    {
        ErrorCatalogCrossValidator validator = new();

        ErrorProfileCatalogDocument profileCatalog = new()
        {
            Profiles =
            [
                new ErrorProfileDefinition
                {
                    ExcludeErrors = null!
                }
            ]
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
            issue => issue.Code == "ProfileExcludeErrorsCollectionIsNull");

        Assert.Equal("profiles[0].excludeErrors", issue.Path);
        Assert.Equal(
            "Profile exclude errors collection is null.",
            issue.Message);
    }
}
