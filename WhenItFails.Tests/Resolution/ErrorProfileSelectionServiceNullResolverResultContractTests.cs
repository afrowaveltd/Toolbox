using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Resolution;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Resolution;

public sealed class ErrorProfileSelectionServiceNullResolverResultContractTests
{
    [Fact]
    public void ResolveByProfileName_ShouldReturnInvalidResponse_WhenResolverReturnsNull()
    {
        ErrorProfileSelectionService service = new(
            new NullResultProfileResolver());

        Response<IReadOnlyList<ErrorDefinition>> response =
            service.ResolveByProfileName(
                CreateContext(),
                "WEB_API");

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Null(response.Data);

        var issue = Assert.Single(response.Issues);
        Assert.Equal("WIF_PROFILE_RESOLVER_RESULT_NULL", issue.Code);
        Assert.Equal(
            "The error profile resolver returned a null result.",
            response.Message);
    }

    private static ErrorCatalogContext CreateContext()
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
                    DisplayName = "Web API"
                }
            ]
        };

        return new ErrorCatalogContext
        {
            ErrorCatalog = new ErrorCatalog(errorCatalogDocument.Errors),
            ErrorCatalogDocument = errorCatalogDocument,
            CategoryCatalog = new ErrorCategoryCatalogDocument(),
            CodeGroupCatalog = new ErrorCodeGroupCatalogDocument(),
            OwnerCatalog = new ErrorOwnerCatalogDocument(),
            ProfileCatalog = profileCatalog,
            CrossValidationResult = new ErrorCatalogValidationResult()
        };
    }

    private sealed class NullResultProfileResolver : IErrorProfileResolver
    {
        public IReadOnlyList<ErrorDefinition> Resolve(
            ErrorCatalogDocument errorCatalog,
            ErrorProfileDefinition profile)
        {
            return null!;
        }
    }
}
