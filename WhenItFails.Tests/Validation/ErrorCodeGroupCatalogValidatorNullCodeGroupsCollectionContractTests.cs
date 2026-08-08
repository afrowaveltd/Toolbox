using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Validation;

public sealed class ErrorCodeGroupCatalogValidatorNullCodeGroupsCollectionContractTests
{
    [Fact]
    public void Validate_ShouldReturnInvalidResult_WhenCodeGroupCollectionIsNull()
    {
        ErrorCodeGroupCatalogDocument document = new()
        {
            CatalogId = "test-code-groups",
            CatalogName = "Test code groups",
            CodeGroups = null!
        };

        ErrorCodeGroupCatalogValidator validator = new();

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.False(result.IsValid);

        ErrorCatalogValidationIssue issue = Assert.Single(
            result.Issues,
            issue => issue.Code == "CodeGroupCatalogCodeGroupsCollectionIsNull");

        Assert.Equal("codeGroups", issue.Path);
        Assert.Equal("Code group catalog code groups collection is null.", issue.Message);
    }
}
