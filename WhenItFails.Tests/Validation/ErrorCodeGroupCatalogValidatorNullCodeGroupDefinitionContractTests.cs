using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Validation;

public sealed class ErrorCodeGroupCatalogValidatorNullCodeGroupDefinitionContractTests
{
    [Fact]
    public void Validate_ShouldReturnInvalidResult_WhenCodeGroupCollectionContainsNullDefinition()
    {
        ErrorCodeGroupCatalogValidator validator = new();

        ErrorCodeGroupCatalogDocument document = new()
        {
            CodeGroups = [null!]
        };

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.False(result.IsValid);

        var issue = Assert.Single(
            result.Issues,
            issue => issue.Code == "CodeGroupDefinitionIsNull");

        Assert.Equal("codeGroups[0]", issue.Path);
        Assert.Equal(
            "Code group catalog contains a null code group definition.",
            issue.Message);
    }
}
