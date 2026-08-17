using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Validation;

public sealed class ErrorCatalogCrossValidatorNullOwnerAliasesCollectionContractTests
{
    [Fact]
    public void Validate_ShouldReturnInvalidResult_WhenOwnerAliasesCollectionIsNull()
    {
        ErrorCatalogCrossValidator validator = new();

        ErrorOwnerCatalogDocument ownerCatalog = new()
        {
            Owners =
            [
                new ErrorOwnerDefinition
                {
                    Name = "AFW",
                    DisplayName = "Afrowave",
                    Aliases = null!
                }
            ]
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
            issue => issue.Code == "OwnerAliasesCollectionIsNull");

        Assert.Equal("ownerCatalog.owners[0].aliases", issue.Path);
        Assert.Equal(
            "Owner aliases collection is null.",
            issue.Message);
    }
}
