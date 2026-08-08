using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Validation;

public sealed class ErrorProfileCatalogValidatorNullProfilesCollectionContractTests
{
    [Fact]
    public void Validate_ShouldReturnInvalidResult_WhenProfileCollectionIsNull()
    {
        ErrorProfileCatalogValidator validator = new();

        ErrorProfileCatalogDocument document = new()
        {
            Profiles = null!
        };

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.False(result.IsValid);

        var issue = Assert.Single(
            result.Issues,
            issue => issue.Code == "ProfileCatalogProfilesCollectionIsNull");

        Assert.Equal("profiles", issue.Path);
        Assert.Equal(
            "Profile catalog profiles collection is null.",
            issue.Message);
    }
}
