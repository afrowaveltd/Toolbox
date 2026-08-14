using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Validation;

public sealed class ErrorProfileCatalogValidatorNullIncludeOwnersCollectionContractTests
{
    [Fact]
    public void Validate_ShouldReturnInvalidResult_WhenIncludeOwnersCollectionIsNull()
    {
        ErrorProfileCatalogValidator validator = new();

        ErrorProfileCatalogDocument document = new()
        {
            Profiles =
            [
                new ErrorProfileDefinition
                {
                    IncludeOwners = null!
                }
            ]
        };

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.False(result.IsValid);

        var issue = Assert.Single(
            result.Issues,
            issue => issue.Code == "ProfileIncludeOwnersCollectionIsNull");

        Assert.Equal("profiles[0].includeOwners", issue.Path);
        Assert.Equal(
            "Profile include owners collection is null.",
            issue.Message);
    }
}
