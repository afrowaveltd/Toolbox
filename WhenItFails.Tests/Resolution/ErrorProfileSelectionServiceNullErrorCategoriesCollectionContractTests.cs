using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Resolution;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Resolution;

public sealed class ErrorProfileSelectionServiceNullErrorCategoriesCollectionContractTests
{
    [Fact]
    public void ResolveByProfileName_WhenErrorCategoriesCollectionIsNull_ReturnsInvalidResponse()
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
                    PrimaryCategory = "OTHER",
                    Categories = null!,
                    Subcategories = [],
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
                    IncludeCategories = ["NETWORK"]
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
            "Error categories collection is null.",
            response.Message);

        Assert.Collection(
            response.Issues,
            issue =>
            {
                Assert.Equal(
                    "ErrorCategoriesCollectionIsNull",
                    issue.Code);
                Assert.Equal(
                    "Error categories collection is null.",
                    issue.Message);
            });
    }
}
