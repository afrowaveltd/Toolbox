using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Validation;

public sealed class ErrorCatalogCrossValidatorNullCodeGroupsCollectionContractTests
{
    [Fact]
    public void Validate_ShouldReturnInvalidResult_WhenCodeGroupCatalogCodeGroupsCollectionIsNull()
    {
        ErrorCatalogCrossValidator validator = new();

        ErrorCodeGroupCatalogDocument codeGroupCatalog = new()
        {
            CodeGroups = null!
        };

        ErrorCatalogValidationResult result = validator.Validate(
            new ErrorCatalogDocument(),
            new ErrorOwnerCatalogDocument(),
            codeGroupCatalog,
            new ErrorCategoryCatalogDocument(),
            new ErrorProfileCatalogDocument());

        Assert.False(result.IsValid);

        var issue = Assert.Single(
            result.Issues,
            issue => issue.Code == "CodeGroupCatalogCodeGroupsCollectionIsNull");

        Assert.Equal("codeGroupCatalog.codeGroups", issue.Path);
        Assert.Equal(
            "Code group catalog code groups collection is null.",
            issue.Message);
    }
}
