using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Validation;

public sealed class ErrorCatalogCrossValidatorNullProfileIncludeErrorsCollectionContractTests
{
    [Fact]
    public void Validate_ShouldReturnInvalidResult_WhenProfileIncludeErrorsCollectionIsNull()
    {
        ErrorCatalogCrossValidator validator = new();

        ErrorProfileCatalogDocument profileCatalog = new()
        {
            Profiles =
            [
                new ErrorProfileDefinition
                {
                    IncludeErrors = null!
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
            issue => issue.Code == "ProfileIncludeErrorsCollectionIsNull");

        Assert.Equal("profiles[0].includeErrors", issue.Path);
        Assert.Equal(
            "Profile include errors collection is null.",
            issue.Message);
    }
}
