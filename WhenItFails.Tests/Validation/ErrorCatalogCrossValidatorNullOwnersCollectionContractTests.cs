using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Validation;

public sealed class ErrorCatalogCrossValidatorNullOwnersCollectionContractTests
{
    [Fact]
    public void Validate_ShouldReturnInvalidResult_WhenOwnerCatalogOwnersCollectionIsNull()
    {
        ErrorCatalogCrossValidator validator = new();

        ErrorOwnerCatalogDocument ownerCatalog = new()
        {
            Owners = null!
        };

        ErrorCatalogValidationResult result = validator.Validate(
            new ErrorCatalogDocument(),
            ownerCatalog,
            new ErrorCodeGroupCatalogDocument(),
            new ErrorCategoryCatalogDocument(),
            new ErrorProfileCatalogDocument());

        Assert.False(result.IsValid);

        var issue = Assert.Single(
            result.Issues,
            issue => issue.Code == "OwnerCatalogOwnersCollectionIsNull");

        Assert.Equal("ownerCatalog.owners", issue.Path);
        Assert.Equal(
            "Owner catalog owners collection is null.",
            issue.Message);
    }
}
