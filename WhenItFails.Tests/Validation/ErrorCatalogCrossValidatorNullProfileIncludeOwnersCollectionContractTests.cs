using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Validation;

public sealed class ErrorCatalogCrossValidatorNullProfileIncludeOwnersCollectionContractTests
{
    [Fact]
    public void Validate_ShouldReturnInvalidResult_WhenProfileIncludeOwnersCollectionIsNull()
    {
        ErrorCatalogCrossValidator validator = new();

        ErrorOwnerCatalogDocument ownerCatalog = new()
        {
            Owners =
            [
                new ErrorOwnerDefinition
                {
                    Name = "AFW",
                    DisplayName = "Afrowave"
                }
            ]
        };

        ErrorProfileCatalogDocument profileCatalog = new()
        {
            Profiles =
            [
                new ErrorProfileDefinition
                {
                    IncludeOwners = null!
                }
            ]
        };

        ErrorCatalogValidationResult result = validator.Validate(
            new ErrorCatalogDocument(),
            ownerCatalog,
            new ErrorCodeGroupCatalogDocument(),
            new ErrorCategoryCatalogDocument(),
            profileCatalog);

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
