using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Resolution;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Resolution;

public sealed class ErrorProfileSelectionServiceResolverExceptionContractTests
{
    [Fact]
    public void ResolveByProfileName_WhenResolverThrows_ReturnsStableFailureWithoutExceptionDetail()
    {
        ErrorProfileSelectionService service = new(
            new ThrowingProfileResolver());

        Response<IReadOnlyList<ErrorDefinition>> response =
            service.ResolveByProfileName(
                CreateContext(),
                "WEB_API");

        Assert.NotNull(response);
        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Failed, response.Status);
        Assert.Null(response.Data);
        Assert.Equal(
            "The error profile resolver failed.",
            response.Message);
        Assert.DoesNotContain(
            "Sensitive profile resolver detail must not escape.",
            response.Message,
            StringComparison.Ordinal);

        Assert.Collection(
            response.Issues,
            issue =>
            {
                Assert.Equal(
                    "WIF_PROFILE_RESOLVER_FAILED",
                    issue.Code);
                Assert.Equal(
                    "The error profile resolver failed.",
                    issue.Message);
                Assert.DoesNotContain(
                    "Sensitive profile resolver detail must not escape.",
                    issue.Message,
                    StringComparison.Ordinal);
            });
    }

    [Fact]
    public void ResolveByProfileName_WhenResolverCancels_RethrowsSameOperationCanceledException()
    {
        OperationCanceledException cancellation =
            new("Expected profile resolver cancellation.");

        ErrorProfileSelectionService service = new(
            new CancellingProfileResolver(cancellation));

        OperationCanceledException thrown =
            Assert.Throws<OperationCanceledException>(
                () => service.ResolveByProfileName(
                    CreateContext(),
                    "WEB_API"));

        Assert.Same(cancellation, thrown);
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

    private sealed class ThrowingProfileResolver : IErrorProfileResolver
    {
        public IReadOnlyList<ErrorDefinition> Resolve(
            ErrorCatalogDocument errorCatalog,
            ErrorProfileDefinition profile)
        {
            throw new InvalidOperationException(
                "Sensitive profile resolver detail must not escape.");
        }
    }

    private sealed class CancellingProfileResolver(
        OperationCanceledException cancellation)
        : IErrorProfileResolver
    {
        public IReadOnlyList<ErrorDefinition> Resolve(
            ErrorCatalogDocument errorCatalog,
            ErrorProfileDefinition profile)
        {
            throw cancellation;
        }
    }
}
