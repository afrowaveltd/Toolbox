using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Validation;

public sealed class ErrorCodeGroupCatalogValidatorNullDefaultCategoriesCollectionContractTests
{
    [Fact]
    public void Validate_ShouldReturnInvalidResult_WhenDefaultCategoriesCollectionIsNull()
    {
        ErrorCodeGroupCatalogValidator validator = new();

        ErrorCodeGroupCatalogDocument document = new()
        {
            CodeGroups =
            [
                new ErrorCodeGroupDefinition
                {
                    DefaultCategories = null!
                }
            ]
        };

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.False(result.IsValid);

        var issue = Assert.Single(
            result.Issues,
            issue => issue.Code == "CodeGroupDefaultCategoriesCollectionIsNull");

        Assert.Equal("codeGroups[0].defaultCategories", issue.Path);
        Assert.Equal(
            "Code group default categories collection is null.",
            issue.Message);
    }
}
