using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Validation;

public sealed class ErrorOwnerCatalogValidatorNullOwnerDefinitionContractTests
{
    [Fact]
    public void Validate_ShouldReturnInvalidResult_WhenOwnerCollectionContainsNullDefinition()
    {
        ErrorOwnerCatalogValidator validator = new();

        ErrorOwnerCatalogDocument document = new()
        {
            Owners = [null!]
        };

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.False(result.IsValid);

        var issue = Assert.Single(
            result.Issues,
            issue => issue.Code == "OwnerDefinitionIsNull");

        Assert.Equal("owners[0]", issue.Path);
        Assert.Equal(
            "Owner catalog contains a null owner definition.",
            issue.Message);
    }
}
