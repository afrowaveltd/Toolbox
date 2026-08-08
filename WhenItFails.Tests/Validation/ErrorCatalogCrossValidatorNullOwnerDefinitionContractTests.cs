using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Validation;

public sealed class ErrorCatalogCrossValidatorNullOwnerDefinitionContractTests
{
    [Fact]
    public void Validate_ShouldReturnInvalidResult_WhenOwnerCatalogContainsNullDefinition()
    {
        ErrorCatalogCrossValidator validator = new();

        ErrorOwnerCatalogDocument ownerCatalog = new()
        {
            Owners = [null!]
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
            issue => issue.Code == "OwnerDefinitionIsNull");

        Assert.Equal("ownerCatalog.owners[0]", issue.Path);
        Assert.Equal(
            "Owner catalog contains a null owner definition.",
            issue.Message);
    }
}
