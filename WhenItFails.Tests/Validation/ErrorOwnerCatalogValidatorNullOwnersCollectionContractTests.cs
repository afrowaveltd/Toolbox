using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Validation;

public sealed class ErrorOwnerCatalogValidatorNullOwnersCollectionContractTests
{
    [Fact]
    public void Validate_ShouldReturnInvalidResult_WhenOwnerCollectionIsNull()
    {
        ErrorOwnerCatalogValidator validator = new();

        ErrorOwnerCatalogDocument document = new()
        {
            Owners = null!
        };

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.False(result.IsValid);

        var issue = Assert.Single(
            result.Issues,
            issue => issue.Code == "OwnerCatalogOwnersCollectionIsNull");

        Assert.Equal("owners", issue.Path);
        Assert.Equal(
            "Owner catalog owners collection is null.",
            issue.Message);
    }
}
