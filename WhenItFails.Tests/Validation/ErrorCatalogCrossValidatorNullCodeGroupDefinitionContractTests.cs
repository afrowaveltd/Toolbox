using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Validation;

public sealed class ErrorCatalogCrossValidatorNullCodeGroupDefinitionContractTests
{
    [Fact]
    public void Validate_ShouldReturnInvalidResult_WhenCodeGroupCatalogContainsNullDefinition()
    {
        ErrorCatalogCrossValidator validator = new();

        ErrorCodeGroupCatalogDocument codeGroupCatalog = new()
        {
            CodeGroups = [null!]
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
            issue => issue.Code == "CodeGroupDefinitionIsNull");

        Assert.Equal("codeGroupCatalog.codeGroups[0]", issue.Path);
        Assert.Equal(
            "Code group catalog contains a null code group definition.",
            issue.Message);
    }
}
