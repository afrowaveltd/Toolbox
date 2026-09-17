using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Resolution;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Resolution;

public sealed class ErrorProfileSelectionServiceNullErrorSubcategoriesCollectionContractTests
{
    [Fact]
    public void ResolveByProfileName_WhenErrorSubcategoriesCollectionIsNull_ReturnsInvalidResponse()
    {
        ErrorCatalogDocument errorCatalogDocument = new()
        {
            Errors =
            [
                new ErrorDefinition
                {
                    Id = "AFW-NET-0001",
                    Code = 100_001,
                    Name = "NetworkFailure",
                    Owner = "AFW",
                    CodePrefix = "NET",
                    CodeGroup = "NETWORK",
                    PrimaryCategory = "NETWORK",
                    Categories = [],
                    Subcategories = null!,
                    Tags = []
                }
            ]
        };

        ErrorProfileCatalogDocument profileCatalog = new()
        {
            Profiles =
            [
                new ErrorProfileDefinition
                {
                    Name = "WEB_API",
                    DisplayName = "Web API",
                    IncludeSubcategories = ["NETWORK"]
                }
            ]
        };

        ErrorCatalogContext context = new()
        {
            ErrorCatalog = new ErrorCatalog([]),
            ErrorCatalogDocument = errorCatalogDocument,
            CategoryCatalog = new ErrorCategoryCatalogDocument(),
            CodeGroupCatalog = new ErrorCodeGroupCatalogDocument(),
            OwnerCatalog = new ErrorOwnerCatalogDocument(),
            ProfileCatalog = profileCatalog,
            CrossValidationResult = new ErrorCatalogValidationResult()
        };

        ErrorProfileSelectionService service = new(
            new ErrorProfileResolver());

        Response<IReadOnlyList<ErrorDefinition>> response =
            service.ResolveByProfileName(
                context,
                "WEB_API");

        Assert.NotNull(response);
        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Null(response.Data);
        Assert.Equal(
            "Error subcategories collection is null.",
            response.Message);

        Assert.Collection(
            response.Issues,
            issue =>
            {
                Assert.Equal(
                    "ErrorSubcategoriesCollectionIsNull",
                    issue.Code);
                Assert.Equal(
                    "Error subcategories collection is null.",
                    issue.Message);
            });
    }
}
