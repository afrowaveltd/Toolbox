using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Validation;

public sealed class ErrorCatalogCrossValidatorNullProfilesCollectionContractTests
{
    [Fact]
    public void Validate_ShouldReturnInvalidResult_WhenProfileCatalogProfilesCollectionIsNull()
    {
        ErrorCatalogCrossValidator validator = new();

        ErrorProfileCatalogDocument profileCatalog = new()
        {
            Profiles = null!
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
            issue => issue.Code == "ProfileCatalogProfilesCollectionIsNull");

        Assert.Equal("profileCatalog.profiles", issue.Path);
        Assert.Equal(
            "Profile catalog profiles collection is null.",
            issue.Message);
    }
}
