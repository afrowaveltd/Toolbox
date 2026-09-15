using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Resolution;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Resolution;

public sealed class ErrorProfileSelectionServiceNullErrorDefinitionContractTests
{
    [Fact]
    public void ResolveByProfileName_WhenErrorCatalogContainsNullDefinition_ReturnsInvalidResponse()
    {
        ErrorCatalogDocument errorCatalogDocument = new()
        {
            Errors =
            [
                null!
            ]
        };

        ErrorProfileCatalogDocument profileCatalog = new()
        {
            Profiles =
            [
                new ErrorProfileDefinition
                {
                    Name = "WEB_API",
                    DisplayName = "Web API"
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
            "Error catalog contains a null error definition.",
            response.Message);

        Assert.Collection(
            response.Issues,
            issue =>
            {
                Assert.Equal(
                    "ErrorDefinitionIsNull",
                    issue.Code);
                Assert.Equal(
                    "Error catalog contains a null error definition.",
                    issue.Message);
            });
    }
}
