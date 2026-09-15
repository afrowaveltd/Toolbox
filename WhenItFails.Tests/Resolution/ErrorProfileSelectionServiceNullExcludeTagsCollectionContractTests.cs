using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Resolution;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Resolution;

public sealed class ErrorProfileSelectionServiceNullExcludeTagsCollectionContractTests
{
    [Fact]
    public void ResolveByProfileName_WhenProfileExcludeTagsCollectionIsNull_ReturnsInvalidResponse()
    {
        ErrorCatalogDocument errorCatalogDocument = new()
        {
            Errors =
            [
                new ErrorDefinition
                {
                    Id = "AFW-WEB-0001",
                    Code = 100_001,
                    Name = "WebRequestFailed"
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
                    ExcludeTags = null!
                }
            ]
        };

        ErrorCatalogContext context = new()
        {
            ErrorCatalog = new ErrorCatalog(errorCatalogDocument.Errors),
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
            "Profile exclude tags collection is null.",
            response.Message);

        Assert.Collection(
            response.Issues,
            issue =>
            {
                Assert.Equal(
                    "ProfileExcludeTagsCollectionIsNull",
                    issue.Code);
                Assert.Equal(
                    "Profile exclude tags collection is null.",
                    issue.Message);
            });
    }
}
