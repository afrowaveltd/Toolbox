using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Validation;

public sealed class ErrorOwnerCatalogValidatorNullAliasesCollectionContractTests
{
    [Fact]
    public void Validate_ShouldReturnInvalidResult_WhenOwnerAliasesCollectionIsNull()
    {
        ErrorOwnerCatalogValidator validator = new();

        ErrorOwnerCatalogDocument document = new()
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

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.False(result.IsValid);

        var issue = Assert.Single(
            result.Issues,
            issue => issue.Code == "OwnerAliasesCollectionIsNull");

        Assert.Equal("owners[0].aliases", issue.Path);
        Assert.Equal(
            "Owner aliases collection is null.",
            issue.Message);
    }
}
